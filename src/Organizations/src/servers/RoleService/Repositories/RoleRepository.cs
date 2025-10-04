
using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Svc.Roles.Models;

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
  public async Task<Role> GetByIdAsync(string org_Id, string id) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_Id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var collection = GetCollection(org_Id);
    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString()),
      Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
    );
    return await collection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Role> CreateAsync(string org_Id, Role role) {
    var collection = GetCollection(org_Id);
    await collection.InsertOneAsync(role);
    return role;
  }

  public async Task<Role> UpdateAsync(string org_Id, string id, Role updaterole) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_Id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }
    var collection = GetCollection(org_Id);

    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString()),
      Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
      );
    var updateDataBuilder = Builders<Role>.Update;
    var updates = new List<UpdateDefinition<Role>>();

    if (!string.IsNullOrWhiteSpace(updaterole.Name))
      updates.Add(updateDataBuilder.Set(r => r.Name, updaterole.Name));

    if (!string.IsNullOrWhiteSpace(updaterole.Description))
      updates.Add(updateDataBuilder.Set(r => r.Description, updaterole.Description));

    if (updaterole.Permissions != null && updaterole.Permissions.Count > 0)
      updates.Add(updateDataBuilder.Set(r => r.Permissions, updaterole.Permissions.ToList()));

    if (updates.Count == 0) {
      return null;
    }
    var update = updateDataBuilder.Combine(updates);
    var result = await collection.UpdateOneAsync(filter, update);

    if (result.MatchedCount == 0) {
      return null;
    }
    return await collection.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<bool> TryUpdateAsync(string orgId, string roleId, Role updateRole) {
    // 1. RoleModel に名前等の null チェック用のプロパティを追加
    
    // 2. MongoDB から変更しようとしている最新の組織のロールを取得する
    var latestRoll = await GetByIdAsync(orgId, roleId);
    // 3. 2 と編集されたロールの差分があるかチェック
    
    // 4. 差分がなければ何もしない
    // 5. 差分があれば、Builders を更新する

    if (!ObjectId.TryParse(roleId, out var objectId) || !ObjectId.TryParse(orgId, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var collection = GetCollection(orgId);

    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString()),
      Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
      );
    var updateDataBuilder = Builders<Role>.Update;
    var updates = new List<UpdateDefinition<Role>>();

    if (!string.IsNullOrWhiteSpace(updateRole.Name))
      updates.Add(updateDataBuilder.Set(r => r.Name, updateRole.Name));

    if (!string.IsNullOrWhiteSpace(updateRole.Description))
      updates.Add(updateDataBuilder.Set(r => r.Description, updateRole.Description));

    if (updateRole.Permissions != null && updateRole.Permissions.Count > 0)
      updates.Add(updateDataBuilder.Set(r => r.Permissions, updateRole.Permissions.ToList()));

    if (updates.Count == 0) {
      return false;
    }
    var update = updateDataBuilder.Combine(updates);
    var result = await collection.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      return false;
    }

    return true;
  }

  public async Task<bool> DeleteAsync(string org_Id, string id) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_Id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var collection = GetCollection(org_Id);
    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString()),
      Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
    );
    var result = await collection.DeleteOneAsync(filter);
    return result.DeletedCount > 0;
  }
}
