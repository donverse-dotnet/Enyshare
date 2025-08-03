namespace Pocco.Svc.EventBridge.Contexts.Models;

public enum EventType {
  OnAccountCreated,
  OnAccountUpdated,
  OnAccountModerated,
  OnAccountDisabled,

  OnOrganizationCreated,
  OnOrganizationUpdated,
  OnOrganizationModerated,
  OnOrganizationDisabled,
  OnOrganizationChatCreated,
  OnOrganizationChatUpdated,
  OnOrganizationChatDeleted,
  OnOrganizationRoleCreated,
  OnOrganizationRoleUpdated,
  OnOrganizationRoleDeleted,
  OnOrganizationMemberJoined,
  OnOrganizationMemberUpdated,
  OnOrganizationMemberLeft,
  OnOrganizationMessageCreated,
  OnOrganizationMessageUpdated,
  OnOrganizationMessageDeleted,
  OnOrganizationMessageReactionAdded,
  OnOrganizationMessageReactionRemoved,
}
