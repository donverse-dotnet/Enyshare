using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Accounts.Models;


namespace Pocco.Svc.Accounts.Services;


public class UserAccountsService : V0AccountService.V0AccountServiceBase {
  private readonly IMongoCollection<Account> _accounts;

  public UserAccountsService(IMongoClient mongoClient) {
    var database = mongoClient.GetDatabase("Entities");
    _accounts = database.GetCollection<Account>("Accounts");
  }

  public override async Task<V0RegisterReply> Register(V0RegisterRequest request, ServerCallContext context) {
    var model = new Account {
      Email = request.Email,
      IsEmailVerified = false,
      PasswordHash = request.Password,
      Username = request.Email.Split('@')[0],
      Role = "User",
      IsActive = true,

      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    await _accounts.InsertOneAsync(model);
    return new V0RegisterReply {
      Success = true,
      Message = "Account Register successfully."
    };
  }

  public override async Task<V0UpdateReply> Update(V0UpdateRequest request, ServerCallContext context) {

    var filter = Builders<Account>.Filter.Eq(a => a.Id, ObjectId.Parse(request.Id));
    var updateDataBuilder = Builders<Account>.Update;
    var updates = new List<UpdateDefinition<Account>>();

    // 1.パスワード以外を更新
    updates.Add(updateDataBuilder
    .Set(acc => acc.Username, request.Name)
    .Set(acc => acc.Email, request.Email)
    .Set(acc => acc.AvatarUrl, request.AvatarUrl)
    .Set(acc => acc.Status, new V0AccountStatusMessageWrapper(request.Status.Status, request.Status.Message))
    .Set(acc => acc.Notifications, new V0AccountNotificationSettingWrapper(
      request.Notifications.Email,
      request.Notifications.Push,
      request.Notifications.ShowBadge
    ))
    );
    // 2.パスワードを更新
    if (!string.IsNullOrEmpty(request.Password) || !string.IsNullOrEmpty(request.OnetimeCode)) {
      //  2.1　ワンタイムコードを比較
      var account = _accounts
      .Find(acc => acc.Id == ObjectId.Parse(request.Id) && acc.Onetimecode == request.OnetimeCode)
      .FirstOrDefault();

      if (account is null) {
        return new V0UpdateReply {
          Success = false,
          Message = "Invalid one-time code or accound not found."
        };
      }
      //  2.2　パスワードを更新
      updates.Add(updateDataBuilder.Set(acc => acc.PasswordHash, request.Password));
    }
    // 3.データベースを更新
    var result = await _accounts.UpdateOneAsync(filter, updateDataBuilder.Combine(updates));
    // 4.リプライ
    if (result.ModifiedCount > 0) {
      return new V0UpdateReply {
        Success = true,
        Message = "Account updated successfully."
      };
    } else {
      return new V0UpdateReply {
        Success = false,
        Message = "No account was updated. Check if the ID is correct."
      };
    }
  }

  public override async Task<V0DeleteReply> Delete(V0DeleteRequest request, ServerCallContext context) {

    var filter = Builders<Account>.Filter.Eq(a => a.Id, ObjectId.Parse(request.Id));
    var updateDataBuilder = Builders<Account>.Update;
    var updates = new List<UpdateDefinition<Account>>() {
      updateDataBuilder.Set(acc => acc.IsActive, false),
      updateDataBuilder.Set(acc => acc.UpdatedAt, DateTime.UtcNow),
      updateDataBuilder.Set(acc => acc.DeletionRequestedAt, DateTime.UtcNow)
    };

    var result = await _accounts.UpdateOneAsync(filter, updateDataBuilder.Combine(updates));

    if (result.ModifiedCount > 0) {
      return new V0DeleteReply {
        Success = true,
        Message = "Account deleted successfully."
      };
    } else {
      return new V0DeleteReply {
        Success = false,
        Message = "Account not found or already deleted."
      };
    }
  }
}
