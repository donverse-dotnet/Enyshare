using MongoDB.Bson;

namespace Pocco.Svc.Roles.Models;

public class Role {
    public ObjectId Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string Permissions { get; set; }

}