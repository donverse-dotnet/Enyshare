using Grpc.Core;
using Pocco.Svc.Accounts.Protos;

namespace Pocco.Svc.Accounts.Services;

public class UserAccountsService : UserAccounts.UserAccountsBase {
  public override async Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context) {
    string email = request.Email;
    string password = request.Password;
    






    return new RegisterAccountReply();
  }

  public override async Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {
    string name = request.Name;
    byte[] iconBytes = request.IconData.ToByteArray();
    string statusMessage = request.StatusMessage;
    string password = request.Password;
    string email = request.Email;
    bool notice = request.Notice;
    return new UpdateAccountReply();
  }

  public override async Task<DeleteAccountReply> DeleteAccount(DeleteAccountRequest request, ServerCallContext context) {
    string accountId = request.AccountId;
    return new DeleteAccountReply();
  }
}
