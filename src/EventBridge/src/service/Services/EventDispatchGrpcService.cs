using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Utilities;

namespace Pocco.Svc.EventBridge.Services;

public class EventDispatchGrpcService(
  [FromServices] EventStoreTasksDeployer eventStoreTasksDeployer,
  [FromServices] EventDeployInvoker eventDeployInvoker,
  [FromServices] EventSender eventSender,
  [FromServices] ILogger<EventDispatchGrpcService> logger
) : Events.EventsBase {
  private readonly ILogger<EventDispatchGrpcService> _logger = logger;
  private readonly EventStoreTasksDeployer _eventStoreTasksDeployer = eventStoreTasksDeployer;
  private readonly EventDeployInvoker _eventDeployInvoker = eventDeployInvoker;
  private readonly EventSender _eventSender = eventSender;

  public override async Task<DeployEventResponse> DeployEvent(DeployEventRequest request, ServerCallContext context) {
    // Receive
    _logger.LogInformation("Received DeployEvent request: {Request}", request);
    // Create id
    var uuid = Guid.NewGuid().ToString();
    var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var index = Random.Shared.Next(100000, 999999);
    var id = $"{unix}_{uuid}_{index}";
    // Save event data to database
    var isSaved = await _eventStoreTasksDeployer.SaveEventDataAsync(
      id,
      request.EventDataCase,
      GrpcServiceHelper.GetEventData(request)
    );
    // If save event data failed, return error
    if (!isSaved) {
      _logger.LogError("Failed to save event data for request: {Request}", request);
      throw new RpcException(new Status(StatusCode.Internal, "Failed to save event data"));
    }

    // Add event deploy queue
    var isQueued = await _eventDeployInvoker.AddEventToQueueAsync(
      id,
      request
    );
    // If add event to queue failed, return error
    if (!isQueued) {
      _logger.LogError("Failed to add event to queue for request: {Request}", request);
      throw new RpcException(new Status(StatusCode.Internal, "Failed to add event to queue"));
    }

    // Return response
    _logger.LogInformation("Event added to queue successfully for request: {Request}", request);
    return new DeployEventResponse {
      EventId = id,
    };
  }

  public override async Task SubscribeEventStream(SubscribeEventStreamRequest request, IServerStreamWriter<SubscribeEventStreamData> responseStream, ServerCallContext context) {
    _logger.LogInformation("Client subscribed to event stream: {Request}", request);

    while (context.CancellationToken.IsCancellationRequested is false) {
      // Add client to event sender
      await _eventSender.AddClientAsync(request.AccountId, responseStream);

      // Wait for cancellation
      await Task.Delay(1000, context.CancellationToken);

      // Check if the client is still connected
      if (context.CancellationToken.IsCancellationRequested) {
        _logger.LogInformation("Client disconnected: {AccountId}", request.AccountId);
        break;
      }
    }

    // Remove client from event sender when the stream is closed
    await _eventSender.RemoveClientAsync(request.AccountId);
    _logger.LogInformation("Client removed from event stream: {AccountId}", request.AccountId);

    await Task.CompletedTask;
  }
}
