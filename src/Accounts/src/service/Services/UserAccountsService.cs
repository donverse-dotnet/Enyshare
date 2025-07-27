using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Svc.Accounts.Helpers;
using Pocco.Svc.Accounts.Protos.Account;
using Pocco.Svc.Accounts.Mappers;
using Pocco.Svc.Accounts.Models;


namespace Pocco.Svc.Accounts.Services;


public class UserAccountsService : UserAccounts.UserAccountsBase {
  private readonly IMongoCollection<Account> _accounts;
  private readonly IMongoCollection<Setting> _accountsettings;

  public UserAccountsService(IMongoClient mongoClient) {
    var database = mongoClient.GetDatabase("Entities");
    _accounts = database.GetCollection<Account>("Accounts");
    _accountsettings = database.GetCollection<Setting>("AccountSettings");
  }

  public override async Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context) {
    var hashed = PasswordHelper.Hash(request.Password);
    var model = new Account {
      Email = request.Email,
      PasswordHash = hashed,
      CreateAt = DateTime.UtcNow,
      IsEmailVerified = false
    };

    await _accounts.InsertOneAsync(model);
    return new RegisterAccountReply {
      Succes = true,
      Message = "Account Registed successfully."
    };
  }

  public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {

    var userId = request.UserId;
    var filter = Builders<Account>.Filter.Eq(a => a.id, ObjectId.Parse(request.Id));
    var updateBuilder = Builders<Account>.Update;
    var updates = new List<UpdateDefinition<Account>>();

    updates.Add(updateBuilder.Set(a => a.Username, request.Name)
    .Set(a => a.Email, request.Email)
    .Set(a => a.Avatarurl, request.IconData)
    .Set(a => a.Statusmessage, request.StatusMessage)
    .Set(a => a.Role, request.Role)
    .Set(a => a.IsActive, request.IsActive)
    .Set(a => a.PasswordHash, request.Password));

    if (request.UpdateAt) {
      updates.Add(updateBuilder.Set(a => a.UpdateAt, DateTime.UtcNow));
    }


    if (request.PasswordUpdateAt) {
      updates.Add(updateBuilder.Set(a => a.PasswordUpdateAt, DateTime.UtcNow));
    }


    if (request.EmailUpdateAt) {
      updates.Add(updateBuilder.Set(a => a.EmailUpdateAt, DateTime.UtcNow));
    }


    if (request.LastLoginAt) {
      updates.Add(updateBuilder.Set(a => a.LastLoginAt, DateTime.UtcNow));
    }


    var update = updateBuilder.Combine(updates);
    var result = await _accounts.UpdateOneAsync(filter, update);

    var uiSetting = AccountSettingMapper.ToModel(request.AccountUisettings, userId);
    uiSetting.UserId = userId;

    var uiFilter = Builders<Setting>.Filter.Eq(u => u.UserId, userId);
    await _accountsettings.ReplaceOneAsync(uiFilter, uiSetting, new ReplaceOptions { IsUpsert = true });

    if (result.ModifiedCount > 0) {
      return new UpdateAccountReply {
        Succes = true,
        Message = "Account updated successfully."
      };
    } else {
      return new UpdateAccountReply {
        Succes = false,
        Message = "No account was updated. Check if the ID is correct."
      };
    }


  }

  public override async Task<DeleteAccountReply> DeleteAccount(DeleteAccountRequest request, ServerCallContext context) {
    if (!ObjectId.TryParse(request.AccountId, out var objectId)) {
      return new DeleteAccountReply {
        Succes = false,
        Message = "Invalid account ID format."
      };
    }

    var result = await _accounts.DeleteOneAsync(a => a.id == objectId);

    if (result.DeletedCount > 0) {
      return new DeleteAccountReply {
        Succes = true,
        Message = "Account deleted successfully."
      };
    } else {
      return new DeleteAccountReply {
        Succes = false,
        Message = "Account not found or already deleted."
      };
    }
  }
}
