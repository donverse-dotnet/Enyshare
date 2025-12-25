using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_ROLE_CREATED = "OnOrganizationRoleCreated";
    public record OnOrganizationRoleCreated(string EventId, Role Role) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_ROLE_UPDATED = "OnOrganizationRoleUpdated"; // TODO: 名前ではなく情報すべての更新に変更する
    public record OnOrganizationRoleUpdated(string EventId, Role NewRole) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_ROLE_DELETED = "OnOrganizationRoleDeleted";
    public record OnOrganizationRoleDeleted(string EventId, string RoleId) : BaseEvent(EventId);
}
