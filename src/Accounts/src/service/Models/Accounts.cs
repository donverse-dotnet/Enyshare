using MongoDB.Bson;

using Pocco.Libs.Protobufs.Accounts.Enums;

namespace Pocco.Svc.Accounts.Models;

public class Account {
  public ObjectId Id { get; set; }
  public required string Email { get; set; }
  public required string PasswordHash { get; init; }
  public bool IsEmailVerified { get; set; } = false;
  public string Onetimecode { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string AvatarUrl { get; set; } = string.Empty;
  public V0AccountStatusMessageWrapper Status { get; set; } = new(V0AccountStatus.V0Offline, "Account is active");
  public string Role { get; set; } = string.Empty;
  public bool IsActive { get; set; } = false;


  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  public DateTime? DeletionRequestedAt { get; set; } = DateTime.UtcNow;
  public DateTime? LastLoginedAt { get; set; } = DateTime.UtcNow;

  public V0AccountNotificationSettingWrapper Notifications { get; set; } = new(true, true, true);
}
