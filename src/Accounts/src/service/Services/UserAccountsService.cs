using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Google.Protobuf;

using Grpc.Core;

using Microsoft.AspNetCore.Identity;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Svc.Accounts.Protos;
using Pocco.Svc.Accounts.Users;
using Pocco.Svc.Accounts.UsersSettings;

namespace Pocco.Svc.Accounts.Services;


public class UserAccountsService : UserAccounts.UserAccountsBase {
  private readonly IMongoCollection<Account> _accounts;
  private readonly IMongoCollection<Setting> _accountsettings;

  public UserAccountsService(IMongoClient mongoClient) {
    var database = mongoClient.GetDatabase("Entities");
    _accounts = database.GetCollection<Account>("Accounts");
    _accountsettings = database.GetCollection<Setting>("Settings");
  }

  private readonly PasswordHasher<object> _hasher = new();

  public UserAccountsService(IMongoClient mongoClient)
  {
    IMongoDatabase _entitiesDb = database.GetDatabase("Entities");
    var acounts = _entitiesDb.GetCollection<Account>("Accounts");
    var accountSettings = _entitiesDb.GetCollection<Setting>("AccountSettings");
  }

  public override async Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context)
  {
    var hashedPassword = _hasher.HashPassword(null, request.Password);
    var model = new Account {
      Email = request.Email,
      PasswordHash = hashedPassword,
      Onetimecode = request.Onetimecode,
      CreateAt = DateTime.UtcNow,
      IsEmailVerified = false
    };

    await _accounts.InsertOneAsync(model);
    return new RegisterAccountReply();
  }

  public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {

    var filter = Builders<Account>.Filter.Eq(a => a.id, ObjectId.Parse(request.Id));

    var update = Builders<Account>.Update
        .Set(a => a.Username, request.Username)
        .Set(a => a.Avatarurl, request.Avatarurl)
        .Set(a => a.Statusmessage, request.Statusmessage)
        .Set(a => a.Role, request.Role)
        .Set(a => a.IsActive, request.IsActive);

    /*,
    ,
     = request.StatusMessage,
    Role = request.Role,
    IsActive = request.IsActive,
    UpdateAt = request.UpdateAt,
    PasswordUpdateAt = request.PasswordUpdateAt,
    EmailUpdateAt = request.EmailUpdateAt,
    LastLoginAt = request.LastLoginAt,
  };*/

    var result = await _accounts.UpdateOneAsync(filter, update);

    if (result.ModifiedCount > 0) {
      return new UpdateAccountReply
      {
        Succes = true,
        Message = "Account updated successfully."
      };
    }
    await _accountsCollection.InsertOneAsync(accounts);
    return new UpdateAccountReply();
  }

  public override async Task<DeleteAccountReply> DeleteAccount(DeleteAccountRequest request, ServerCallContext context) {
    var accounts = new Account {
      DeletionRequestAt = request.DeletionRequestAt
    };
    await _acountsCollection.InsertOneAsync(accounts);
    return new DeleteAccountReply();
  }
}
