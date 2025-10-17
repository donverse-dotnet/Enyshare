using System.Threading.Channels;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services;

public class EventSendHelper : IHotStartableService {
  public EventSendHelper([FromServices] ILogger<EventSendHelper> logger, [FromServices] EventIdProvider eventIdProvider) {
    _logger = logger;
    _cts = new CancellationTokenSource();
    _eventIdProvider = eventIdProvider;
    _eventQueue = Channel.CreateUnbounded<V0DeployedEventData>();
    _eventReader = _eventQueue.Reader.ReadAllAsync(_cts.Token);
    _logger.LogInformation("EventSendHelper initialized");
  }

  // Event listeners (key: userId, value: list of sessions)
  public readonly Dictionary<string, IServerStreamWriter<V0DeployedEventData>> Writers = [];

  private readonly ILogger<EventSendHelper> _logger;
  private readonly CancellationTokenSource _cts;
  private readonly EventIdProvider _eventIdProvider;
  // Event activator
  private Task? _queueWorker;
  // Event queue
  private readonly Channel<V0DeployedEventData> _eventQueue;
  private readonly IAsyncEnumerable<V0DeployedEventData> _eventReader;

  public async Task WarmUpAsync(IServiceProvider sp, CancellationToken cancellationToken) {
    _logger.LogInformation("EventSendHelper warming up...");

    _queueWorker = Task.Run(async () => await ProcessQueueAsync(_cts.Token), cancellationToken);

    _logger.LogInformation("EventSendHelper warmed up.");
    await Task.CompletedTask;
  }

  public Task CoolDownAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("EventSendHelper cooling down...");

    // Stop the queue worker
    _cts.Cancel();
    try {
      _queueWorker?.Wait(cancellationToken);
    } catch (OperationCanceledException) {
      // Expected when cancellation is requested
      _logger.LogInformation("Queue worker cancelled.");
    } catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException)) {
      // Expected when cancellation is requested
      _logger.LogInformation("Queue worker cancelled.");
    } catch (Exception ex) {
      _logger.LogError(ex, "Error stopping queue worker.");
    }

    _logger.LogInformation("EventSendHelper cooled down.");
    return Task.CompletedTask;
  }

  public string AddListener(IServerStreamWriter<V0DeployedEventData> writer) {
    var uuid = Guid.NewGuid().ToString();

    lock (Writers) {
      Writers.Add(uuid, writer);
      _logger.LogInformation("Added listener: Total listeners: {Count}", Writers.Count);
    }

    return uuid;
  }

  public async Task<string> EnqueueEventAsync(V0NewEventRequest eventData) {
    var eventId = _eventIdProvider.GenerateEventId();
    var deployedEventData = new V0DeployedEventData {
      Topic = eventData.Topic,
      EventType = eventData.EventType,
      EventId = eventId,
      ApiVersion = eventData.ApiVersion,
      InvokedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = eventData.InvokedBy,
      Payload = eventData.Payload,
    };

    await _eventQueue.Writer.WriteAsync(deployedEventData);
    _logger.LogInformation("Enqueued event: EventId={EventId}, EventType={EventType}", deployedEventData.EventId, deployedEventData.EventType);

    return eventId;
  }

  public void RemoveListener(string uuid) {
    lock (Writers) {
      if (Writers.Remove(uuid)) {
        _logger.LogInformation("Removed listener: Total listeners: {Count}", Writers.Count);
      } else {
        _logger.LogWarning("Attempted to remove non-existent listener: {Uuid}", uuid);
      }
    }
  }

  private async Task ProcessQueueAsync(CancellationToken cancellationToken) {
    await foreach (var eventData in _eventReader.WithCancellation(cancellationToken)) {
      // Send event to all listeners
      foreach (var writer in Writers.ToArray()) {
        try {
          await writer.Value.WriteAsync(eventData, cancellationToken);
        } catch (Exception ex) {
          _logger.LogError(ex, "Error sending event to listener. Removing listener.");
          RemoveListener(writer.Key);
        }
      }
    }
  }
}
