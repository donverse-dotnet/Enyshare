
using System.Data;

using Google.Protobuf.WellKnownTypes;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Pocco.Libs.Protobufs.Types;

namespace Pocco.Svc.Chats.Models;

public class Chat {
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public required string Id { get; set; }

  [BsonElement("OrgId")]
  public required string OrgId { get; set; }

  [BsonElement("name")]
  [BsonRequired]
  public required string Name { get; set; }

  [BsonElement("description")]
  [BsonRequired]
  public required string Description { get; set; } = string.Empty;

  [BsonElement("createdBy")]
  [BsonRequired]
  public required string CreatedBy { get; set; } = string.Empty;

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("createdAt")]
  public DateTime CreatedAt { get; set; }

  [BsonElement("isprivate")]
  public bool IsPrivate { get; set; } = false;

  public bool HasName => !string.IsNullOrWhiteSpace(Name);

  public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

  public bool HasCreatedBy => !string.IsNullOrWhiteSpace(CreatedBy);

  public bool IsNameChanged(string name) {
    return Name != name;
  }

  public bool IsDescriptionChanged(string description) {
    return Description != description;
  }

  public bool IsCreatedByChanged(string createdby) {
    return CreatedBy != createdby;
  }

  public V0ChatsModel ToV0ChatModel() {
    return new V0ChatsModel {
      Id = Id,
      OrgId = OrgId,
      Name = Name,
      Description = Description,
      CreatedBy = CreatedBy,
      IsPrivate = IsPrivate,
      CreatedAt = Timestamp.FromDateTime(CreatedAt)
    };
  }
}
