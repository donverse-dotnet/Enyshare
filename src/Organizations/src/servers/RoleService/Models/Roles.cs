
using System.ComponentModel.DataAnnotations;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Roles.Models;

public class Role {
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public required string Id { get; set; }

  // [BsonElement("org_id")]
  // [BsonRepresentation(BsonType.ObjectId)]
  // public string Org_Id { get; set; } = string.Empty;

  [BsonElement("name")]
  [BsonRequired]
  public required string Name { get; set; }

  [BsonElement("discription")]
  [BsonRequired]
  public string Description { get; set; } = string.Empty;

  [BsonElement("permissions")]
  [BsonRequired]
  public List<string> Permissions { get; set; } = new();

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("CreatedAt")]
  public DateTime Created_At { get; set; }

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("UpdatedAt")]
  public DateTime Updated_At { get; set; }

  public bool HasName => !string.IsNullOrWhiteSpace(Name);

  public bool HasDiscription => !string.IsNullOrWhiteSpace(Description);

  public bool HasParmissions => Permissions.Count > 0;

  public bool IsNameChanged(string name) {
    return Name != name;
  }

  public bool IsDescriptionChanged(string discription) {
    return Description != discription;
  }

  public bool IsParmissionChanged(List<string> parmissions) {
    return Permissions != parmissions;
  }
}
