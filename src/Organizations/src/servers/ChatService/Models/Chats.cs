
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Chats.Models;

public class Chat {
  [BsonId]
  [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
  public required string Id { get; set; }

  [BsonElement("name")]
  [BsonRequired]
  public required string Name { get; set; }

  [BsonElement("description")]
  [BsonRequired]
  public required string Description { get; set; } = string.Empty;

  [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
  public string Created_By { get; set; } = default!;

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("created_At")]
  public DateTime Created_At { get; set; }

  [BsonElement("member_ids")]
  public IEnumerable<string> Member_Ids { get; set; } = new List<string>();

  [BsonElement("is_private")]
  public bool Is_Private { get; set; } = false;

  public bool HasName => !string.IsNullOrWhiteSpace(Name);

  public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
}
