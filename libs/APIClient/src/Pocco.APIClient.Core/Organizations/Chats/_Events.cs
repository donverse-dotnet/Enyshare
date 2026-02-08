using Pocco.APIClient.Core.Models;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_CHAT_CREATED = "OnOrganizationChatCreated";
    public record OnOrganizationChatCreated(string EventId, Chat Chat) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_CHAT_UPDATED = "OnOrganizationChatUpdated"; // TODO: 名前ではなく情報すべての更新に変更する
    public record OnOrganizationChatUpdated(string EventId, Chat Chat) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_CHAT_DELETED = "OnOrganizationChatDeleted";
    public record OnOrganizationChatDeleted(string EventId, OrganizationItemDeletedModel Chat) : BaseEvent(EventId);
}
