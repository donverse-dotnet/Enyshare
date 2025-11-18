using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public static partial class ClientEvents {
    public record OnOrganizationCreated(string EventId, Organization Organization) : BaseEvent(EventId);
    public record OnOrganizationNameUpdated(string EventId, string Latest, string Old) : BaseEvent(EventId);
    public record OnOrganizationDeleted(string EventId, string OrganizationId) : BaseEvent(EventId);
}
