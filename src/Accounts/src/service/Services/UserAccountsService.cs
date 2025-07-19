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

  public override async Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context)
  {
    var user = new Account {
      Email = request.Email,
    };
    var hashedPassword = _hasher.HashPassword(user, request.Password);
    var model = new Account {
      Email = request.Email,
      PasswordHash = hashedPassword,
      CreateAt = DateTime.UtcNow,
      IsEmailVerified = false
    };

    await _accounts.InsertOneAsync(model);
    return new RegisterAccountReply();
  }

  public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {

    var filter = Builders<Account>.Filter.Eq(a => a.id, ObjectId.Parse(request.Id));
    var updateBuilder = Builders<Account>.Update;
    var updates = new List<UpdateDefinition<Account>>();

    updates.Add(updateBuilder.Set(a => a.Username, request.Name));
    updates.Add(updateBuilder.Set(a => a.Email, request.Email));
    updates.Add(updateBuilder.Set(a => a.Avatarurl, request.IconData));
    updates.Add(updateBuilder.Set(a => a.Statusmessage, request.StatusMessage));
    updates.Add(updateBuilder.Set(a => a.Role, request.Role));
    updates.Add(updateBuilder.Set(a => a.IsActive, request.IsActive));
    updates.Add(updateBuilder.Set(a => a.PasswordHash, request.Password));

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
