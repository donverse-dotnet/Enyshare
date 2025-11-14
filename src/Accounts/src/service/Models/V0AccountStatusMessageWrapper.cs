using Pocco.Libs.Protobufs.Accounts.Enums;
using Pocco.Libs.Protobufs.Accounts.Types;

namespace Pocco.Svc.Accounts.Models;

public class V0AccountStatusMessageWrapper(V0AccountStatus status, string message) {
  public V0AccountStatus Status { get; set; } = status;
  public string Message { get; set; } = message;

  public V0AccountStatusModel ToV0AccountStatusMessage() {
    return new V0AccountStatusModel {
      Status = Status,
      Message = Message
    };
  }
}
