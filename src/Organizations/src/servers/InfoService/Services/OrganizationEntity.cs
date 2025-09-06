using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InfoService.Services;

public class OrganizationEntity
{
  [BsonId]
  public string Id { get; set; }

  [BsonElement("name")]
  public string Name { get; set; }

  [BsonElement("description")]
  public string Description { get; set; }

  [Bsonelement("createdBy")]
  public string CreatedBy { get; set; }

  [BsonElement("createdAt")]
  public DataTime CreatedAt { get; set; }

  [BsonElement("updatedAt")]
  public DateTime UpdatedAt { get; set; }

  [BsonElement("deletedAt")]
  public DateTime? DeletedAt { get; set; }
}
