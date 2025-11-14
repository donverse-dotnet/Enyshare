using Pocco.Libs.Protobufs.Accounts.Types;

namespace Pocco.Svc.Accounts.Models;

public class V0AccountNotificationSettingWrapper(bool email, bool push, bool showbadge) {
  public bool Email { get; set; } = email;
  public bool Push { get; set; } = push;
  public bool ShowBadge { get; set; } = showbadge;

  public V0AccountNotificationSettings ToV0AccountStatusMessage() {
    return new V0AccountNotificationSettings {
      Email = Email,
      Push = Push,
      ShowBadge = ShowBadge
    };
  }
}
