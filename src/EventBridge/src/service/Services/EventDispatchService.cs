using Grpc.Core;

namespace Pocco.Svc.EventBridge.Services;

public class EventDispatchService : Events.EventsBase {
  public override Task<DeployEventResponse> DeployEvent(DeployEventRequest request, ServerCallContext context) {
    return base.DeployEvent(request, context);
  }

  public override Task SubscribeEventStream(SubscribeEventStreamRequest request, IServerStreamWriter<SubscribeEventStreamData> responseStream, ServerCallContext context) {
    return base.SubscribeEventStream(request, responseStream, context);
  }
}
