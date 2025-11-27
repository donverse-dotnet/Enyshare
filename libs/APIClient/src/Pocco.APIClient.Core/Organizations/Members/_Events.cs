using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_MEMBER_JOINED = "OnOrganizationMemberCreated";
    public record OnOrganizationMemberCreated(string EventId, Member Member) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_MEMBER_LEAVED = "OnOrganizationMemberDeleted";
    public record OnOrganizationMemberDeleted(string EventId, string MemberId) : BaseEvent(EventId);
}
