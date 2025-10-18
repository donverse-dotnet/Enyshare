
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Chats.Models;

public class Chat {
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public required string Id { get; set; }

  [BsonElement("name")]
  [BsonRequired]
  public required string Name { get; set; }

  [BsonElement("description")]
  [BsonRequired]
  public required string Description { get; set; } = string.Empty;

  [BsonRepresentation(BsonType.ObjectId)]
  public string CreatedBy { get; set; } = default!;

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("createdAt")]
  public DateTime CreatedAt { get; set; }

  [BsonElement("memberids")]
  public IEnumerable<string> MemberIds { get; set; } = new List<string>();

  [BsonElement("isprivate")]
  public bool IsPrivate { get; set; } = false;

  public bool HasName => !string.IsNullOrWhiteSpace(Name);

  public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

  public bool IsNameChanged(string name) {
    return Name != name;
  }

  public bool IsDescriptionChanged(string description) {
    return Description != description;
  }
}
