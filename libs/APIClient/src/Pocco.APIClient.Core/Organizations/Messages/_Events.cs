using Pocco.APIClient.Core.Models;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public const string ON_ORGANIZATION_MESSAGE_CREATED = "OnOrganizationSendMessage";
    public record OnOrganizationSendMessage(string EventId, Message Message) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_MESSAGE_UPDATED = "OnOrganizationUpdateMessage"; // TODO: 名前ではなく情報すべての更新に変更する
    public record OnOrganizationUpdateMessage(string EventId, Message NewMessage) : BaseEvent(EventId);

    public const string ON_ORGANIZATION_MESSAGE_DELETED = "OnOrganizationDeleteMessage";
    public record OnOrganizationDeleteMessage(string EventId, OrganizationOnItemDeletedModel Message) : BaseEvent(EventId);
}
