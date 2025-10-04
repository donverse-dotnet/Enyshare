
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

  private IMongoCollection<Role> GetRoleCollection(string org_Id) {
    var db = _client.GetDatabase(org_Id);
    return db.GetCollection<Role>("Roles");
  }
  public async Task<Role> GetByIdAsync(string org_Id, string id) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_Id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var roles = GetRoleCollection(org_Id);
    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString())
      // Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
    );
    return await roles.Find(filter).FirstOrDefaultAsync();
  }

  public async Task<Role> CreateAsync(string org_Id, Role role) {
    var roles = GetRoleCollection(org_Id);
    await roles.InsertOneAsync(role);

    var isCreated = await GetByIdAsync(org_Id, role.Id);

    if (isCreated is null) {
      throw new RpcException(new Status(StatusCode.Internal, $"Role creation failed for {org_Id}"));
    }

    return isCreated;
  }

  public async Task<bool> TryUpdateAsync(string orgId, string roleId, Role updateRole) {
    // 1. RoleModel に名前等の null チェック用のプロパティを追加

    // 2. MongoDB から変更しようとしている最新の組織のロールを取得する
    var latestRoll = await GetByIdAsync(orgId, roleId);
    // 3. 2 と編集されたロールの差分があるかチェック
    var isNameChanged = updateRole.IsNameChanged(latestRoll.Name);
    var isDescriptionChanged = updateRole.IsDescriptionChanged(latestRoll.Description);
    var isParmissionChanged = updateRole.IsParmissionChanged(latestRoll.Permissions);
    // 4. 差分がなければ何もしない
    if (isNameChanged == false && isDescriptionChanged == false && isParmissionChanged == false) {
      return false;
    }
    // 5. 差分があれば、Builders を更新する
    
    if (!ObjectId.TryParse(roleId, out var objectId) || !ObjectId.TryParse(orgId, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var roles = GetRoleCollection(orgId);

    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString())
      // Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
      );
    var updateDataBuilder = Builders<Role>.Update;
    var updates = new List<UpdateDefinition<Role>>();

    if (isNameChanged)
      updates.Add(updateDataBuilder.Set(r => r.Name, updateRole.Name));

    if (isDescriptionChanged)
      updates.Add(updateDataBuilder.Set(r => r.Description, updateRole.Description));

    if (isParmissionChanged)
      updates.Add(updateDataBuilder.Set(r => r.Permissions, updateRole.Permissions.ToList()));

 
    var update = updateDataBuilder.Combine(updates);
    var result = await roles.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      return false;
    }

    return true;
  }

  public async Task<bool> DeleteAsync(string org_Id, string id) {
    if (!ObjectId.TryParse(id, out var objectId) || !ObjectId.TryParse(org_Id, out var orgObjectId)) {
      throw new ArgumentException("Invalid id or orgId format");
    }

    var roles = GetRoleCollection(org_Id);
    var filter = Builders<Role>.Filter.And(
      Builders<Role>.Filter.Eq(r => r.Id, objectId.ToString())
      // Builders<Role>.Filter.Eq(r => r.Org_Id, orgObjectId.ToString())
    );
    var result = await roles.DeleteOneAsync(filter);
    return result.DeletedCount > 0;
  }
}
