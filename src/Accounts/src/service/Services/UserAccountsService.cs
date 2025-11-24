using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Accounts.Enums;
using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.Accounts.Types;
using Pocco.Svc.Accounts.Models;

namespace Pocco.Svc.Accounts.Services;

public class UserAccountsService : V0AccountService.V0AccountServiceBase {
  private readonly IMongoCollection<Account> _accounts;

  public UserAccountsService(IMongoClient mongoClient) {
    var database = mongoClient.GetDatabase("Entities");
    _accounts = database.GetCollection<Account>("Accounts");
  }

  public override async Task<V0GetAccountReply> Get(V0GetAccountRequest request, ServerCallContext context) {
    var account = await _accounts.FindAsync(acc => acc.Id == ObjectId.Parse(request.Id)).Result.FirstOrDefaultAsync()
                ?? throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

    var status = account.Status.ToV0AccountStatusMessage();
    var returnAccountData = new V0AccountBaseModel {
      Id = account.Id.ToString(),
      Username = account.Username,
      AvatarUrl = account.AvatarUrl,
      Status = status,
      CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(account.CreatedAt.ToUniversalTime())
    };

    Console.WriteLine($"[Get Account] ID: {account.Id}, Username: {account.Username}, Status: {status.Status}, CreatedAt: {account.CreatedAt}");

    return new V0GetAccountReply {
      Account = returnAccountData
    };
  }

  public override async Task<V0RegisterAccountReply> Register(V0RegisterAccountRequest request, ServerCallContext context) {
    var existingAccount = await _accounts.FindAsync(acc => acc.Email == request.Email).Result.FirstOrDefaultAsync();
    if (existingAccount is not null) {
      throw new RpcException(new Status(StatusCode.AlreadyExists, "An account with this email already exists."));
    }

    var currentTime = DateTime.UtcNow;

    var model = new Account {
      Email = request.Email,
      IsEmailVerified = false,
      PasswordHash = request.Password,
      Username = request.Email.Split('@')[0],
      Role = "User",
      IsActive = true,

      CreatedAt = currentTime,
      UpdatedAt = currentTime
    };

    await _accounts.InsertOneAsync(model);
    return new V0RegisterAccountReply {
      Success = true,
      Message = "Account Register successfully."
    };
  }

  public override async Task<V0UpdateAccountReply> Update(V0UpdateAccountRequest request, ServerCallContext context) {

    var filter = Builders<Account>.Filter.Eq(a => a.Id, ObjectId.Parse(request.NewAccount.Id));
    var updateDataBuilder = Builders<Account>.Update;
    var updates = new List<UpdateDefinition<Account>> {
      // 1.パスワード以外を更新
      updateDataBuilder
        .Set(acc => acc.Username, request.NewAccount.Username)
        .Set(acc => acc.Email, request.NewAccount.Email)
        .Set(acc => acc.AvatarUrl, request.NewAccount.AvatarUrl)
        .Set(acc => acc.Status, new V0AccountStatusMessageWrapper(request.NewAccount.Status.Status, request.NewAccount.Status.Message))
        .Set(acc => acc.Notifications, new V0AccountNotificationSettingWrapper(
          request.NewAccount.Notifications.Email,
          request.NewAccount.Notifications.Push,
          request.NewAccount.Notifications.ShowBadge
        ))
        .Set(acc => acc.LastLoginedAt, DateTime.UtcNow)
    };

    // 2.パスワードを更新
    // if (!string.IsNullOrEmpty(request.Password) || !string.IsNullOrEmpty(request.OnetimeCode)) {
    //   //  2.1　ワンタイムコードを比較
    //   var account = _accounts
    //   .Find(acc => acc.Id == ObjectId.Parse(request.Id) && acc.OnetimeCode == request.OnetimeCode)
    //   .FirstOrDefault();

    //   if (account is null) {
    //     return new V0UpdateReply {
    //       Success = false,
    //       Message = "Invalid one-time code or account not found."
    //     };
    //   }
    //   //  2.2　パスワードを更新
    //   updates.Add(updateDataBuilder.Set(acc => acc.PasswordHash, request.Password));
    // }

    // 3.データベースを更新
    var result = await _accounts.UpdateOneAsync(filter, updateDataBuilder.Combine(updates));
    // 4.リプライ
    if (result.ModifiedCount > 0) {
      return new V0UpdateAccountReply {
        Account = request.NewAccount,
        Success = true,
      };
    } else {
      return new V0UpdateAccountReply {
        Account = null,
        Success = false,
      };
    }
  }

  public override async Task<V0DeleteAccountReply> Delete(V0DeleteAccountRequest request, ServerCallContext context) {

    var filter = Builders<Account>.Filter.Eq(a => a.Id, ObjectId.Parse(request.Id));
    var updateDataBuilder = Builders<Account>.Update;
    var updates = new List<UpdateDefinition<Account>>() {
      updateDataBuilder.Set(acc => acc.IsActive, false),
      updateDataBuilder.Set(acc => acc.UpdatedAt, DateTime.UtcNow),
      updateDataBuilder.Set(acc => acc.DeletionRequestedAt, DateTime.UtcNow)
    };

    var result = await _accounts.UpdateOneAsync(filter, updateDataBuilder.Combine(updates));

    if (result.ModifiedCount > 0) {
      return new V0DeleteAccountReply {
        Success = true,
        Message = "Account deleted successfully."
      };
    } else {
      return new V0DeleteAccountReply {
        Success = false,
        Message = "Account not found or already deleted."
      };
    }
  }
}
