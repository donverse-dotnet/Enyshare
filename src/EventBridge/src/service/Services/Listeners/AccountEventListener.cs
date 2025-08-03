using Grpc.Core;
using Pocco.Svc.EventBridge.Contexts;
using Pocco.Svc.EventBridge.Protobufs;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services.Listeners;

// TODO: アカウントサービスの実装が完了したら、ここにイベントのリスナーを追加します。
public class AccountEventListener(EventLogContext logContext) : V0AccountEvents.V0AccountEventsBase {
  public override Task<V0PublishResponse> V0PublishAccountCreated(V0AccountCreatedEvent request, ServerCallContext context) {
    return base.V0PublishAccountCreated(request, context);
  }

  public override Task<V0PublishResponse> V0PublishAccountUpdated(V0AccountUpdatedEvent request, ServerCallContext context) {
    return base.V0PublishAccountUpdated(request, context);
  }

  public override Task<V0PublishResponse> V0PublishAccountModerated(V0AccountModeratedEvent request, ServerCallContext context) {
    return base.V0PublishAccountModerated(request, context);
  }

  public override Task<V0PublishResponse> V0PublishAccountDisabled(V0AccountDisabledEvent request, ServerCallContext context) {
    return base.V0PublishAccountDisabled(request, context);
  }
}
