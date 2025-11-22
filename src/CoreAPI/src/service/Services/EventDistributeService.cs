// Add event to StreamHolder event queues

using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Svc.CoreAPI.Services;

public class EventDistributeService : IHotStartableService {
  private readonly ILogger<EventDistributeService> _logger;
  private readonly StreamHolder _streamHolder;
  private static readonly Channel<V0EventData> _channel = Channel.CreateUnbounded<V0EventData>();
  private readonly IAsyncEnumerable<V0EventData> _eventQueue = _channel.Reader.ReadAllAsync();
  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private Task? _processingTask;

  public EventDistributeService([FromServices] ILogger<EventDistributeService> logger, [FromServices] StreamHolder streamHolder) {
    _logger = logger;
    _streamHolder = streamHolder;

    _logger.LogInformation("EventDistributeService initialized.");
  }

  public void EnqueueEvent(V0EventData eventData) {
    _logger.LogInformation("Enqueuing event: Topic={Topic}, Payload={Payload}", eventData.Topic, eventData.Payload);

    _channel.Writer.TryWrite(eventData);
  }

  private async Task ProcessEventQueueAsync(CancellationToken cancellationToken) {
    await foreach (var eventData in _eventQueue.WithCancellation(cancellationToken)) {
      var writers = _streamHolder.GetStreamWriters(item =>
        item.UserId == eventData.InvokedBy ||
        item.Filters.MatchesTopic(eventData.Topic.ToString()) ||
        item.Filters.MatchesOrganizationId(eventData.Payload.Fields["ornigazation_id"].ToString()) ||
        item.Filters.MatchesActiveOrganizationId(eventData.Payload.Fields["ornigazation_id"].ToString()) ||
        item.Filters.MatchesActiveOrganizationChatId(eventData.Payload.Fields["chat_id"].ToString())
      );

      foreach (var writer in writers) {
        _logger.LogInformation("Dispatching event to SessionId={SessionId}, UserId={UserId}",
          writer.SessionId, writer.UserId);
        writer.EnqueueEvent(eventData);
      }

      _logger.LogInformation("Dispatched event: Topic={Topic}, Payload={Payload}, DispatchedTo={Count} writers",
        eventData.Topic, eventData.Payload, writers.Count);
    }
  }

  public async Task WarmUpAsync(IServiceProvider sp, CancellationToken cancellationToken) {
    _logger.LogInformation("EventHandler service is warming up.");

    // Start processing the event queue
    _processingTask = Task.Run(async () => await ProcessEventQueueAsync(_cancellationTokenSource.Token), cancellationToken);

    // No initialization needed
    await Task.CompletedTask;
  }

  public async Task CoolDownAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("EventHandler service is cooling down.");

    try {
      _cancellationTokenSource.Cancel();
      _processingTask?.Wait(cancellationToken);
      _cancellationTokenSource.Dispose();
    } catch (OperationCanceledException) {
      _logger.LogInformation("Event processing task was cancelled.");
    } catch (AggregateException aggEx) {
      foreach (var ex in aggEx.InnerExceptions) {
        if (ex is OperationCanceledException) {
          _logger.LogInformation("Event processing task was cancelled.");
        } else {
          _logger.LogError(ex, "Error occurred while stopping event processing task.");
        }
      }
    }

    _logger.LogInformation("EventHandler service has cooled down.");
    await Task.CompletedTask;
  }
}
