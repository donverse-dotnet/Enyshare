using System.Runtime.CompilerServices;

using Google.Protobuf;

using Grpc.Core;

using Microsoft.AspNetCore.Identity;

using MongoDB.Driver;

using Pocco.Svc.Accounts.Protos;
using Pocco.Svc.Accounts.Users;

namespace Pocco.Svc.Accounts.Services;


public class UserAccountsService : UserAccounts.UserAccountsBase {
  private readonly IMongoCollection<User> _userCollection;
  private readonly PasswordHasher<object> _hasher = new();
  public override async Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context) {
    var hashedPassword = _hasher.HashPassword(null, request.Password);
    var user = new User {
      Email = request.Email,
      PasswordHash = hashedPassword,
      IsEmailVerified = request.IsEmailVerified,
      Onetimecode = request.Onetimecode,
    };

    await _userCollection.InsertOneAsync(user);
    return new RegisterAccountReply();
  }

  public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {
    var user = new Pocco.Svc.Accounts.Users.User {
      Username = request.Username,
      Avatarurl = request.Avatarurl,
      Statusmessage = request.StatusMessage,
      Role = request.Role,
      IsActive = request.IsActive,


    };

    return new UpdateAccountReply();
  }

  public override async Task<DeleteAccountReply> DeleteAccount(DeleteAccountRequest request, ServerCallContext context) {
    string accountId = request.AccountId;
    return new DeleteAccountReply();
  }
}
