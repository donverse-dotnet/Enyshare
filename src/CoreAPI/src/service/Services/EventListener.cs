// Receive events from EventBridge

using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Enums;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.CoreAPI.Services;

public class EventListener : IHotStartableService {

  public EventListener([FromServices] ILogger<EventListener> logger, [FromServices] EventDistributeService eventDistributeService) {
    _logger = logger;
    _eventDistributeService = eventDistributeService;
    _cancellationTokenSource = new CancellationTokenSource();

    var serverAddress = Environment.GetEnvironmentVariable("EVENT_BRIDGE_ADDRESS") ?? throw new InvalidOperationException("EVENT_BRIDGE_ADDRESS environment variable is not set.");
    _channel = GrpcChannel.ForAddress(serverAddress);
    _client = new V0EventDispatcher.V0EventDispatcherClient(_channel);

    _logger.LogInformation("EventListener initialized.");
  }

  private readonly ILogger<EventListener> _logger;
  private readonly EventDistributeService _eventDistributeService;
  private readonly GrpcChannel _channel;
  private readonly V0EventDispatcher.V0EventDispatcherClient _client;
  private AsyncServerStreamingCall<V0EventData>? _streamingCall;
  private readonly CancellationTokenSource _cancellationTokenSource;
  private Task? _listeningTask;

  public async Task WarmUpAsync(IServiceProvider sp, CancellationToken cancellationToken) {
    _logger.LogInformation("EventListener service is warming up.");

    // Start listening to EventBridge
    _listeningTask = Task.Run(async () => await ListenEventBridgeAsync(_cancellationTokenSource.Token), cancellationToken);

    await Task.CompletedTask;
  }

  public async Task CoolDownAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("EventListener service is cooling down.");

    // Cancel the listening task
    _cancellationTokenSource.Cancel();
    try {
      _listeningTask?.Wait(cancellationToken);
    } catch (OperationCanceledException) {
      _logger.LogInformation("Listening task was cancelled.");
    } catch (AggregateException aggEx) {
      foreach (var ex in aggEx.InnerExceptions) {
        if (ex is OperationCanceledException) {
          _logger.LogInformation("Listening task was cancelled.");
        } else {
          _logger.LogError(ex, "An error occurred while stopping the listening task.");
        }
      }
    } finally {
      _streamingCall?.Dispose();
      _channel.ShutdownAsync().Wait(cancellationToken);
      _cancellationTokenSource.Dispose();
    }

    await Task.CompletedTask;
  }

  private async Task ListenEventBridgeAsync(CancellationToken cancellationToken) {
    if (_eventDistributeService == null) {
      _logger.LogError("EventDistributeService is not available. Cannot listen to EventBridge.");
      return;
    }

    while (!cancellationToken.IsCancellationRequested) {
      try {
        _logger.LogInformation("Starting to listen to EventBridge...");

        _streamingCall = _client.Listen(new ListenRequest(), cancellationToken: cancellationToken);
        var responseStream = _streamingCall.ResponseStream;

        while (await responseStream.MoveNext(cancellationToken)) {
          var eventData = responseStream.Current;
          // Process the received event data
          var convertedEvent = new Pocco.Libs.Protobufs.Services.V0EventData {
            EventId = eventData.EventId,
            EventType = eventData.EventType,
            Topic = eventData.Topic switch {
              V0EventTopics.EventTopicUnspecified => Libs.Protobufs.Services.V0EventTopics.EventTopicUnspecified,
              V0EventTopics.EventTopicUser => Libs.Protobufs.Services.V0EventTopics.EventTopicUser,
              V0EventTopics.EventTopicSystem => Libs.Protobufs.Services.V0EventTopics.EventTopicSystem,
              V0EventTopics.EventTopicOrganization => Libs.Protobufs.Services.V0EventTopics.EventTopicOrganization,
              _ => Libs.Protobufs.Services.V0EventTopics.EventTopicUnspecified,
            },
            Payload = eventData.Payload,
            InvokedAt = eventData.InvokedAt,
            InvokedBy = eventData.InvokedBy,
          };
          // convertedEvent.Payload.MergeFrom(eventData.Payload);

          _eventDistributeService.EnqueueEvent(convertedEvent);

          _logger.LogInformation("Received event: {EventId}, Topic: {Topic}", eventData.EventId, eventData.Topic.ToString());
        }
      } catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled) {
        _logger.LogInformation("Event listening was cancelled.");
      } catch (Exception ex) {
        _logger.LogError(ex, "An error occurred while listening to EventBridge.");
      } finally {
        _logger.LogInformation("Stopped listening to EventBridge.");
      }

      // Wait before attempting to reconnect
      await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      _logger.LogInformation("Reconnecting to EventBridge...");
    }
  }
}
