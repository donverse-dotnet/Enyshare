namespace Pocco.APIClient.Core;

public static class Events {
    public const string PRIVATE_EVENT_ID = "PRIVATE_EVENT";

    public record BaseEvent(string EventId);

    public record OnClientLoggedIn(string EventId, SessionData Session) : BaseEvent(EventId);
    public record OnClientLoggedOut(string EventId) : BaseEvent(EventId);
    public record OnSessionExpired(string EventId) : BaseEvent(EventId);
    public record OnSessionRefreshed(string EventId, SessionData Session) : BaseEvent(EventId);
}
