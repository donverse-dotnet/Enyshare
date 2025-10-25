
using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Svc.Roles.Models;

namespace Pocco.Svc.Roles.Repositories;

public class RoleRepository : IRoleRepository {
  private readonly IMongoClient _client;

  public RoleRepository(IMongoClient client) {
    _client = client;
  }

  private FilterDefinition<Role> CreateFilter(string roleId) {
    if (!ObjectId.TryParse(roleId, out _)) {
      throw new ArgumentException("Invalid id or roleId format");
    }

    return Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, roleId)
    );
  }

  private IMongoCollection<Role> GetRoleCollection(string orgId) {
    if (!ObjectId.TryParse(orgId, out _)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var db = _client.GetDatabase(orgId);
    return db.GetCollection<Role>("Roles");
  }

  public async Task<Role> GetByIdAsync(string orgId, string roleId) {
    var roles = GetRoleCollection(orgId);
    var filter = CreateFilter(roleId);
    return await roles.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Role> CreateAsync(string orgId, Role role) {
    var roles = GetRoleCollection(orgId);
    await roles.InsertOneAsync(role);

    var latestRole = await GetByIdAsync(orgId, role.Id);

    if (latestRole is null) {
      throw new RpcException(new Status(StatusCode.Internal, $"Role creation failed for {orgId}"));
    }

    return latestRole;
  }

  public async Task<bool> TryUpdateAsync(string orgId, string roleId, Role newRole) {
    var latestRoll = await GetByIdAsync(orgId, roleId);

    var isNameChanged = newRole.IsNameChanged(latestRoll.Name);
    var isDescriptionChanged = newRole.IsDescriptionChanged(latestRoll.Description);
    var isParmissionChanged = newRole.IsParmissionChanged(latestRoll.Permissions);
    if (!isNameChanged && !isDescriptionChanged && !isParmissionChanged) {
      return false;
    }

    var updateDataBuilder = Builders<Role>.Update;
    var updates = new List<UpdateDefinition<Role>>();

    if (isNameChanged)
      updates.Add(updateDataBuilder.Set(r => r.Name, newRole.Name));

    if (isDescriptionChanged)
      updates.Add(updateDataBuilder.Set(r => r.Description, newRole.Description));

    if (isParmissionChanged)
      updates.Add(updateDataBuilder.Set(r => r.Permissions, newRole.Permissions.ToList()));

    var update = updateDataBuilder.Combine(updates);
    var roles = GetRoleCollection(orgId);
    var filter = CreateFilter(roleId);
    var result = await roles.UpdateOneAsync(filter, update);

    return result.ModifiedCount != 0;
  }

  public async Task<bool> DeleteAsync(string orgId, string roleId) {
    var roles = GetRoleCollection(orgId);
    var filter = CreateFilter(roleId);
    var result = await roles.DeleteOneAsync(filter);
    return result.DeletedCount > 0;
  }
}
