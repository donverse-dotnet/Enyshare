
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Roles.Models;

public class Role {
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string Id { get; set; } = default!;

  [BsonElement("org_id")]
  [BsonRepresentation(BsonType.ObjectId)]
  public string Org_Id { get; set; } = string.Empty;

  [BsonElement("name")]
  [BsonRequired]
  public required string Name { get; set; }

  [BsonElement("discription")]
  [BsonRequired]
  public required string Description { get; set; } = string.Empty;

  [BsonElement("permissions")]
  [BsonRequired]
  public required List<string> Permissions { get; set; } = new();

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("CreatedAt")]
  public DateTime Created_At { get; set; }

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("UpdatedAt")]
  public DateTime Updated_At { get; set; }
}