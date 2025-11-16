#pragma warning disable CS8618
using Google.Protobuf.WellKnownTypes;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core;

public class SessionData {
    public string Token { get; set; }
    public string SessionId { get; set; }
    public string AccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static SessionData FromProto(V0ApiSessionData proto) {
        return new SessionData {
            Token = proto.Token,
            SessionId = proto.SessionId,
            AccountId = proto.AccountId,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(proto.CreatedAt.Seconds).UtcDateTime,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(proto.ExpiresAt.Seconds).UtcDateTime,
            UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(proto.UpdatedAt.Seconds).UtcDateTime
        };
    }

    public static V0ApiSessionData ToProto(SessionData data) {
        return new V0ApiSessionData {
            Token = data.Token,
            SessionId = data.SessionId,
            AccountId = data.AccountId,
            CreatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(data.CreatedAt, DateTimeKind.Utc)),
            ExpiresAt = Timestamp.FromDateTime(DateTime.SpecifyKind(data.ExpiresAt, DateTimeKind.Utc)),
            UpdatedAt = Timestamp.FromDateTime(DateTime.SpecifyKind(data.UpdatedAt, DateTimeKind.Utc))
        };
    }
}
