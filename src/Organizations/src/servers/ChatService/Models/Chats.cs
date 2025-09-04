
using MongoDB.Bson.Serialization.Attributes;

namespace Pocco.Svc.Chats.Models;

public class Chat {
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public required string Id { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }
    public required List<string> Roles { get; set; }

    [BsonIgnoreIfNull]
    public List<string> Member_Ids { get; set; }

    [BsonIgnoreIfNull]
    public bool? Is_Private { get; set; }
}