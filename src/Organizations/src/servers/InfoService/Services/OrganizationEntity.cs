using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InfoService.Services;

public class OrganizationEntity
{
  [BsonId]
  public string Id { get; set; } = string.Empty;

  [BsonId]
  public string ObjectId { get; set; } = string.Empty;

  [BsonElement("name")]
  public string Name { get; set; } = string.Empty;

  [BsonElement("description")]
  public string Description { get; set; } = string.Empty;

  [BsonElement("createdBy")]
  public string CreatedBy { get; set; } = string.Empty;

  [BsonElement("createdAt")]
  public DateTime CreatedAt { get; set; }

  [BsonElement("updatedAt")]
  public DateTime UpdatedAt { get; set; }

  [BsonElement("deletedAt")]
  public DateTime? DeletedAt { get; set; }
}
