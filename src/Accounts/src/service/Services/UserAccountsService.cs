using System.Runtime.CompilerServices;

using Google.Protobuf;

using Grpc.Core;

using Microsoft.AspNetCore.Identity;

using MongoDB.Driver;

using Pocco.Svc.Accounts.Protos;
using Pocco.Svc.Accounts.Users;

namespace Pocco.Svc.Accounts.Services;


public class UserAccountsService : UserAccounts.UserAccountsBase {
  private readonly IMongoCollection<Accounts> _acountsCollection;
  private readonly PasswordHasher<object> _hasher = new();
  public override async Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context) {
    var hashedPassword = _hasher.HashPassword(null, request.Password);
    var accounts = new Pocco.Svc.Accounts.Users.Accounts {
      Email = request.Email,
      PasswordHash = hashedPassword,
      IsEmailVerified = request.IsEmailVerified,
      Onetimecode = request.Onetimecode,
    };

    await _accountsCollection.InsertOneAsync(accounts);
    return new RegisterAccountReply();
  }

  public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {
    var accounts = new Pocco.Svc.Accounts.Users.Accounts {
      Username = request.Username,
      Avatarurl = request.Avatarurl,
      Statusmessage = request.StatusMessage,
      Role = request.Role,
      IsActive = request.IsActive,
      CreateAt = request.CreateAt,
      UpdateAt = request.UpdateAt,
      PasswordUpdateAt = request.PasswordUpdateAt,
      EmailUpdateAt = request.EmailUpdateAt,
      LastLoginAt = request.LastLoginAt,
    };
    await _accountsCollection.InsertOneAsync(accounts);
    return new UpdateAccountReply();
  }

  public override async Task<DeleteAccountReply> DeleteAccount(DeleteAccountRequest request, ServerCallContext context) {
    var accounts = new Pocco.Svc.Accounts.Users.Accounts{
      DeletionRequestAt = request.DeletionRequestAt
    };
    await _acountsCollection.InsertOneAsync(accounts);
    return new DeleteAccountReply();
  }
}
