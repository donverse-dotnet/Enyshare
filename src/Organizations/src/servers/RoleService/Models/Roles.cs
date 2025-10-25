using Google.Protobuf.WellKnownTypes;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using Pocco.Libs.Protobufs.Types;

namespace Pocco.Svc.Roles.Models;

public class Role {
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public required string Id { get; set; }

  [BsonElement("orgid")]
  public required string OrgId { get; set; }

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
  public DateTime CreatedAt { get; set; }

  [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
  [BsonElement("UpdatedAt")]
  public DateTime UpdatedAt { get; set; }

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

  public V0RoleModel ToV0RoleModel() {
    var model = new V0RoleModel {
      Id = Id,
      OrgId = OrgId,
      Name = Name,
      Descriptions = Description,
      CreatedAt = Timestamp.FromDateTime(CreatedAt),
      UpdatedAt = Timestamp.FromDateTime(UpdatedAt)
    };
    model.Permissions.AddRange(Permissions);
    return model;
  }
}
