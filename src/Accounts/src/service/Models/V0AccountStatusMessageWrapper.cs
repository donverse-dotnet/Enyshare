using Pocco.Libs.Protobufs.Enums;
using Pocco.Libs.Protobufs.Types;

namespace Pocco.Svc.Accounts.Models;

public class V0AccountStatusMessageWrapper(V0AccountStatus status, string message) {
    public V0AccountStatus Status { get; set; } = status;
    public string Message { get; set; } = message;

    public V0AccountStatusMessage ToV0AccountStatusMessage() {
        return new V0AccountStatusMessage {
            Status = Status,
            Message = Message
        };
    }
}