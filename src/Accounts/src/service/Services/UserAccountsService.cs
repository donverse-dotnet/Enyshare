using Grpc.Core;
using Pocco.Svc.Accounts.Protos;

namespace Pocco.Svc.Accounts.Services;

public class UserAccountsService : UserAccounts.UserAccountsBase {
  public override Task<RegisterAccountReply> RegisterAccount(RegisterAccountRequest request, ServerCallContext context) {
    string email = request.Email;
    string password = request.Password;
    return base.RegisterAccount(request, context);
  }

  public override Task<UpdateAccountReply> UpdateAccount(UpdateAccountRequest request, ServerCallContext context) {
    string name = request.Name;
    byte[] iconBytes = request.IconData.ToByteArray();
    string statusMessage = request.StatusMessage;
    string password = request.Password;
    string email = request.Email;
    bool notice = request.Notice;
    return base.UpdateAccount(request, context);
  }

  public override Task<DeleteAccountReply> DeleteAccount(DeleteAccountRequest request, ServerCallContext context) {
    string accountId = request.AccountId;
    return base.DeleteAccount(request, context);
  }
}
