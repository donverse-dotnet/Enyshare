using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_INFO_CREATED = "OnOrganizationInfoCreated";
    public record OnOrganizationInfoCreated(string EventId, Organization Organization) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_INFO_UPDATED = "OnOrganizationInfoUpdated"; // TODO: 名前ではなく情報すべての更新に変更する
    public record OnOrganizationInfoUpdated(string EventId, Organization NewOrganization) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_INFO_DELETED = "OnOrganizationInfoDeleted";
    public record OnOrganizationInfoDeleted(string EventId, string OrganizationId) : BaseEvent(EventId);
}
