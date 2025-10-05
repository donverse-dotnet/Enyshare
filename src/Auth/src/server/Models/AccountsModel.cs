using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Srv.Auth.Models;

public class AccountsModel {
  [BsonId]
  public ObjectId Id { get; set; }

  [BsonElement("Email")]
  public string? Email { get; set; }

  [BsonElement("PasswordHash")]
  public string? PasswordHash { get; set; }

  [BsonElement("CreatedAt")]
  public DateTime CreatedAt { get; set; }

  [BsonElement("Role")]
  public string Role { get; set; } = "User";

  [BsonExtraElements]
  public BsonDocument? ExtraElements { get; set; } // 他のフィールド
}
