using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using Pocco.Svc.EventBridge.Utilities;

namespace Pocco.Svc.EventBridge.Services;

public class EventDispatchService(
  [FromServices] EventStoreTasksDeployer eventStoreTasksDeployer,
  [FromServices] EventDeployInvoker eventDeployInvoker,
  ILogger<EventDispatchService> logger
) : Events.EventsBase {
  private readonly ILogger<EventDispatchService> _logger = logger;
  private readonly EventStoreTasksDeployer _eventStoreTasksDeployer = eventStoreTasksDeployer;
  private readonly EventDeployInvoker _eventDeployInvoker = eventDeployInvoker;

  public override async Task<DeployEventResponse> DeployEvent(DeployEventRequest request, ServerCallContext context) {
    // Receive
    _logger.LogInformation("Received DeployEvent request: {Request}", request);

    // Id
    var eventId = Guid.NewGuid().ToString();

    // Save event data
    bool saveEventDataTask = await _eventStoreTasksDeployer.SaveEventDataAsync(
      eventId,
      request.EventDataCase,
      GrpcServiceHelper.GetEventData(request)
    );
    // If save event data success, add queue to deploy event
    // If save event data failed, return error
    if (!saveEventDataTask) {
      _logger.LogError("Failed to save event data for request: {Request}", request);
      throw new RpcException(new Status(StatusCode.Internal, "Failed to save event data"));
    }
    _logger.LogInformation("Event data saved successfully for request: {Request}", request);

    bool addEventToQueueTask = await _eventDeployInvoker.AddEventToQueueAsync(
      eventId,
      request
    );

    // If add event to queue success, return response
    // If add event to queue failed, return error
    if (!addEventToQueueTask) {
      _logger.LogError("Failed to add event to queue for request: {Request}", request);
      throw new RpcException(new Status(StatusCode.Internal, "Failed to add event to queue"));
    }
    _logger.LogInformation("Event added to queue successfully for request: {Request}", request);
    // Remove event data from database
    // TODO

    return new DeployEventResponse() {
      EventId = eventId,
    };
  }

  public override Task SubscribeEventStream(SubscribeEventStreamRequest request, IServerStreamWriter<SubscribeEventStreamData> responseStream, ServerCallContext context) {
    return base.SubscribeEventStream(request, responseStream, context);
  }
}
