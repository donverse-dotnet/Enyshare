using Grpc.Core;
using Pocco.Svc.EventBridge.Protobufs;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services.Listeners;

// TODO: メッセージサービスの実装が完了したら、ここにイベントのリスナーを追加します。
public class OrganizationEventListener : V0OrganizationEvents.V0OrganizationEventsBase {
  public override Task<V0PublishResponse> PublishOrganizationMessageCreated(V0OrganizationMessageCreatedEvent request, ServerCallContext context) {
    return base.PublishOrganizationMessageCreated(request, context);
  }

  public override Task<V0PublishResponse> PublishOrganizationMessageUpdated(V0OrganizationMessageUpdatedEvent request, ServerCallContext context) {
    return base.PublishOrganizationMessageUpdated(request, context);
  }

  public override Task<V0PublishResponse> PublishOrganizationMessageDeleted(V0OrganizationMessageDeletedEvent request, ServerCallContext context) {
    return base.PublishOrganizationMessageDeleted(request, context);
  }

  public override Task<V0PublishResponse> PublishOrganizationMessageReactionAdded(V0OrganizationMessageReactionAddedEvent request, ServerCallContext context) {
    return base.PublishOrganizationMessageReactionAdded(request, context);
  }

  public override Task<V0PublishResponse> PublishOrganizationMessageReactionRemoved(V0OrganizationMessageReactionRemovedEvent request, ServerCallContext context) {
    return base.PublishOrganizationMessageReactionRemoved(request, context);
  }
}
