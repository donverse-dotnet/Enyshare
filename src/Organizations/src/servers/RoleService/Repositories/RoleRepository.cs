using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Roles.Models;

using System.Threading.Tasks;

namespace Pocco.Svc.Roles.Repositories;

public class RoleRepository : IRoleRepository {
    private readonly IMongoClient _client;

    public RoleRepository(IMongoClient client) {
        _client = client;
    }

    private IMongoCollection<Role> GetCollection(string org_Id) {
        var db = _client.GetDatabase(org_Id);
        return db.GetCollection<Role>("Roles");
    }
    public async Task<Role?> GetByIdAsync(string org_Id, string id) {
        var collection = GetCollection(org_Id);
        var filter = Builders<Role>.Filter.Eq("_id", id);
        return await collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Role> CreateAsync(string org_Id, Role role) {
        var collection = GetCollection(org_Id);
        await collection.InsertOneAsync(role);
        return role;
    }

    public async Task<Role> UpdateAsync(string org_Id, Role role) {
        var collection = GetCollection(org_Id);
        var filter = Builders<Role>.Filter.Eq("_id", role.Id);
        await collection.ReplaceOneAsync(filter, role);
        return role;
    }

    public async Task<bool> DeleteAsync(string org_Id, string id) {
        var collection = GetCollection(org_Id);
        var filter = Builders<Role>.Filter.Eq("_id", id);
        var result = await collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}