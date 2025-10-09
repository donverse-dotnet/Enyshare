using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public class EventWriterOrganizationFilter {
  // Is active
  // If false, do not send any events to this organization -> app notification only
  public bool IsActive { get; set; } = false;
  // User opened chat ID
  public string ActiveChatId { get; set; } = string.Empty;
}

public class EventWriterListenFilter {
  // List of event types to listen to
  // If empty, can not receive any events
  // USER, ORG, SYSTEM(admin only)
  public List<V0EventTopics> EventTypes { get; set; } = [];
  // List of organization filters
  public Dictionary<string, EventWriterOrganizationFilter> OrganizationFilters { get; set; } = [];
}

public class EventWriter {
  private readonly ILogger<EventsService> _logger;

  // Session ID = unique writer ID
  public string WriterId { get; set; }
  // User ID
  public string UserId { get; set; }

  // Filter of events
  public EventWriterListenFilter ListenFilter { get; set; } = new();

  // Last active time
  public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
  // Created time
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Writer
  public IServerStreamWriter<V0EventData> Writer { get; set; }

  public EventWriter(string writerId, string userId, IServerStreamWriter<V0EventData> writer, ILogger<EventsService> logger) {
    _logger = logger;
    WriterId = writerId;
    UserId = userId;
    Writer = writer;

    _logger.LogInformation("EventWriter created: {WriterId} for User: {UserId}", WriterId, UserId);
  }

  // Send event to writer
  public async Task<bool> SendEventAsync(V0EventData e) {
    try {
      // TODO: Check filter
      if (ListenFilter.EventTypes.Count > 0 && !ListenFilter.EventTypes.Contains(e.Topic)) {
        _logger.LogDebug("Event type {EventType} not in listen filter for writer {WriterId}", e.EventType, WriterId);
        return false;
      }

      await Writer.WriteAsync(e);
      LastActiveAt = DateTime.UtcNow;
      return true;
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to send event to writer: {WriterId}", WriterId);
      return false;
    }
  }
}

public class EventsService : V0EventsService.V0EventsServiceBase {
  private readonly CancellationTokenSource _cts = new();

  private readonly ILogger<EventsService> _logger;
  private readonly EventBridge.Protobufs.Services.V0EventListener.V0EventListenerClient _eventListenerClient;
  private readonly Task _receiveEventsTask;

  public EventsService(ILogger<EventsService> logger) {
    _logger = logger;

    var eventBridgeAddress = Environment.GetEnvironmentVariable("EVENT_BRIDGE_ADDRESS") ?? "event-bridge:50051";
    var channel = GrpcChannel.ForAddress(eventBridgeAddress);
    _eventListenerClient = new EventBridge.Protobufs.Services.V0EventListener.V0EventListenerClient(channel);
    _receiveEventsTask = Task.Run(() => ReceiveEventsAsync(_cts.Token));

    _logger.LogInformation("EventsService initialized");
  }
  ~EventsService() {
    _cts.Cancel();
    _receiveEventsTask.Wait();
  }

  // list of writers
  private readonly List<EventWriter> _writers = [];
  // async queue for events
  private readonly Queue<V0EventData> _eventQueue = new();
  private readonly SemaphoreSlim _eventQueueSignal = new(100); // max 100 events in queue
  private readonly Lock _writersLock = new();
  private readonly Lock _eventQueueLock = new();

  private async Task ReceiveEventsAsync(CancellationToken cancellationToken = default) {
    _logger.LogInformation("Starting to receive events from Event Bridge");

    var request = new EventBridge.Protobufs.Services.ListenRequest();
    request.Topics.AddRange([
      EventBridge.Protobufs.Services.V0EventTopics.EventTopicUser,
      EventBridge.Protobufs.Services.V0EventTopics.EventTopicOrganization,
      EventBridge.Protobufs.Services.V0EventTopics.EventTopicSystem,
    ]);

    using var call = _eventListenerClient.Listen(request, cancellationToken: cancellationToken);

    try {
      await foreach (var e in call.ResponseStream.ReadAllAsync(cancellationToken: cancellationToken)) {
        // Enqueue event
        lock (_eventQueueLock) {
          _eventQueueSignal.Wait(cancellationToken);

          // Bind to V0EventData for this service
          var le = new V0EventData {
            Topic = (V0EventTopics)e.Topic,
            EventType = e.EventType,
            ApiVersion = e.ApiVersion,
            EventId = e.EventId,
            InvokedAt = e.InvokedAt,
            InvokedBy = e.InvokedBy,
            Payload = e.Payload
          };

          _eventQueue.Enqueue(le);
          _eventQueueSignal.Release();
        }
      }
    } catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) {
      _logger.LogInformation("Event Bridge stream cancelled");
    } catch (Exception ex) {
      _logger.LogError(ex, "Error receiving events from Event Bridge");
    }
  }

  private async Task ProcessEventQueueAsync(CancellationToken cancellationToken = default) {
    _logger.LogInformation("Starting to process event queue");

    while (!cancellationToken.IsCancellationRequested) {
      V0EventData? e = null;

      // Dequeue event
      lock (_eventQueueLock) {
        if (_eventQueue.Count > 0) {
          _eventQueueSignal.Wait(cancellationToken);
          e = _eventQueue.Dequeue();
          _eventQueueSignal.Release();
        }
      }

      if (e != null) {
        // Send event to all writers
        List<EventWriter> writersCopy;
        lock (_writersLock) {
          writersCopy = _writers.ToList();
        }

        foreach (var writer in writersCopy) {
          var sent = await writer.SendEventAsync(e);
          if (sent) {
            _logger.LogDebug("Event {EventId} sent to writer {WriterId}", e.EventId, writer.WriterId);
          } else {
            _logger.LogDebug("Event {EventId} not sent to writer {WriterId}", e.EventId, writer.WriterId);
          }
        }
      } else {
        // No events, wait a bit
        await Task.Delay(100, cancellationToken);
      }
    }
  }

  public override async Task Listen(ListenRequest request, IServerStreamWriter<V0EventData> responseStream, ServerCallContext context) {
    await base.Listen(request, responseStream, context); // TODO: Remove if not needed

    // Create a new writer
    var writerId = Guid.NewGuid().ToString();
    var userId = request.UserId;
    var writer = new EventWriter(writerId, userId, responseStream, _logger);

    // Add to writers list
    lock (_writersLock) {
      _writers.Add(writer);
    }
    _logger.LogInformation("Writer {WriterId} added for User: {UserId}", writerId, userId);

    // Listen for cancellation
    var cancellationToken = context.CancellationToken;
    cancellationToken.Register(() => {
      // Remove writer from list
      lock (_writersLock) {
        _writers.RemoveAll(w => w.WriterId == writerId);

        _logger.LogInformation("Writer {WriterId} removed for User: {UserId}", writerId, userId);
      }
    });

    // Wait until cancellation
    try {
      while (!cancellationToken.IsCancellationRequested) {
        await Task.Delay(1000, cancellationToken);

        // Update last active time
        writer.LastActiveAt = DateTime.UtcNow;

        // Check if writer is still active
        if (context.CancellationToken.IsCancellationRequested) {
          break;
        }
      }
    } catch (TaskCanceledException) {
      // Ignore
    } catch (Exception ex) {
      _logger.LogError(ex, "Error in Listen for Writer: {WriterId}", writerId);
    }
  }

  public override async Task<Empty> UpdateListen(ListenRequest request, ServerCallContext context) {
    // Find writer by user ID and writer ID(session id)
    EventWriter? writer;
    lock (_writersLock) {
      writer = _writers.FirstOrDefault(w => w.UserId == request.UserId && w.WriterId == request.SessionId);
    }
    if (writer == null) {
      _logger.LogWarning("Writer not found for User: {UserId} with SessionId: {SessionId}", request.UserId, request.SessionId);
      throw new RpcException(new Status(StatusCode.NotFound, "Writer not found"));
    }

    // Update listen filter
    writer.ListenFilter.EventTypes.Clear();
    writer.ListenFilter.EventTypes.AddRange(request.Topics.ToList());
    writer.ListenFilter.OrganizationFilters.Clear();
    foreach (var org in request.OrganizationIds) {
      writer.ListenFilter.OrganizationFilters[org] = new EventWriterOrganizationFilter {
        IsActive = request.ActiveOrganizationId == org,
        // ActiveChatId = request.ActiveChatId, // TODO: Implement active chat ID
      };
    }

    _logger.LogInformation("Writer {WriterId} updated listen filter for User: {UserId}", writer.WriterId, writer.UserId);
    return new Empty();
  }

  public override async Task<Empty> Unlisten(UnlistenRequest request, ServerCallContext context) {
    // Find writer by user ID and writer ID(session id)
    EventWriter? writer;
    lock (_writersLock) {
      writer = _writers.FirstOrDefault(w => w.UserId == request.UserId && w.WriterId == request.SessionId);
    }
    if (writer == null) {
      _logger.LogWarning("Writer not found for User: {UserId} with SessionId: {SessionId}", request.UserId, request.SessionId);
      throw new RpcException(new Status(StatusCode.NotFound, "Writer not found"));
    }

    //Cancel the writer by removing from list
    lock (_writersLock) {
      _writers.RemoveAll(w => w.WriterId == writer.WriterId);
      _logger.LogInformation("Writer {WriterId} removed for User: {UserId}", writer.WriterId, writer.UserId);
    }

    return new Empty();
  }
}
