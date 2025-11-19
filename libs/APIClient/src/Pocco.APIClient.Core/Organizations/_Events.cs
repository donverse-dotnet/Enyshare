using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_CREATED = "OnOrganizationCreated";
    public record OnOrganizationCreated(string EventId, Organization Organization) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_NAME_UPDATED = "OnOrganizationNameUpdated"; // TODO: 名前ではなく情報すべての更新に変更する
    public record OnOrganizationNameUpdated(string EventId, string Latest, string Old) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_DELETED = "OnOrganizationDeleted";
    public record OnOrganizationDeleted(string EventId, string OrganizationId) : BaseEvent(EventId);
}
