
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Chats.Models;

public class Chat {
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonElement("org_id")]

    public string Org_Id { get; set; } = default!;

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("description")]
    public required string Description { get; set; }

    [BsonElement("created_By")]
    public required string Created_By { get; set; }

    [BsonElement("created_At")]
    public DateTime Created_At { get; set; }

    [BsonElement("roles")]
    public required List<string> Roles { get; set; }

    [BsonElement("member_ids")]
    public List<string> Member_Ids { get; set; } = new();

    [BsonElement("is_private")]
    public bool Is_Private { get; set; } = false;
}