using MongoDB.Driver;

using Pocco.Svc.Roles.Models;

namespace Pocco.Svc.Roles.Repositories;

public class RoleRepository : IRoleRepository {
    private readonly IMongoCollection<Role> _collection;

    public RoleRepository(IMongoClient client) {
        var db = client.GetDatabase("Organizations");
        _collection = db.GetCollection<Role>("Roles");
    }

    public async Task<List<Role>> GetAllAsync(string organizationId) {
        var filter = Builders<Role>.Filter.Eq(x => x.org_Id, organizationId);
        return await _collection.Find(filter).ToListAsync();
    }
}