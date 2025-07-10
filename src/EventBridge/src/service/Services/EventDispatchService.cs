using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

namespace Pocco.Svc.EventBridge.Services;

public class EventDispatchService(
  [FromServices] EventStoreTasksDeployer eventStoreTasksDeployer,
  [FromServices] EventDeployInvoker eventDeployInvoker,
  ILogger<EventDispatchService> logger
) : Events.EventsBase {
  private readonly ILogger<EventDispatchService> _logger = logger;
  private readonly EventStoreTasksDeployer _eventStoreTasksDeployer = eventStoreTasksDeployer;
  private readonly EventDeployInvoker _eventDeployInvoker = eventDeployInvoker;

  public override Task<DeployEventResponse> DeployEvent(DeployEventRequest request, ServerCallContext context) {
    return base.DeployEvent(request, context);
  }

  public override Task SubscribeEventStream(SubscribeEventStreamRequest request, IServerStreamWriter<SubscribeEventStreamData> responseStream, ServerCallContext context) {
    return base.SubscribeEventStream(request, responseStream, context);
  }
}
