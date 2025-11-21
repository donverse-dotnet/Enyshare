using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_INFO_CREATED = "OnInfoCreated";
    public record OnOrganizationInfoCreated(string EventId, Organization Organization) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_INFO_UPDATED = "OnInfoUpdated"; // TODO: 名前ではなく情報すべての更新に変更する
    public record OnOrganizationInfoUpdated(string EventId, Organization NewOrganization) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_INFO_DELETED = "OnInfoDeleted";
    public record OnOrganizationInfoDeleted(string EventId, string OrganizationId) : BaseEvent(EventId);
}
