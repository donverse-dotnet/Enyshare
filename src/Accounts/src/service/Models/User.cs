using MongoDB.Bson;

namespace Pocco.Svc.Accounts.Users {
  public class User {
    public ObjectId id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? Onetimecode { get; set; }
    public string Username { get; set; }
    public string? Avatarurl { get; set; }
    public string? Statusmessage { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
    public DateTime? PasswordUpdateAt { get; set; } = DateTime.UtcNow;
    public DateTime? EmailUpdateAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletionRequestAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; } = DateTime.UtcNow;
  }
}
