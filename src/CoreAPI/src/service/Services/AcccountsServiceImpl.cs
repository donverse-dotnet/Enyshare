using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public class AccountsServiceImpl : V0AccountsService.V0AccountsServiceBase {
  private readonly ILogger<AccountsServiceImpl> _logger;

  public AccountsServiceImpl(
    [FromServices] ILogger<AccountsServiceImpl> logger
  ) {
    _logger = logger;

    _logger.LogInformation("AccountsServiceImpl initialized.");
  }

  [Authorize(Policy = "RequireGeneral")]
  public override async Task<GetAccountResponse> Get(GetAccountRequest request, ServerCallContext context) {
    _logger.LogInformation("GetAccount called with AccountId: {AccountId}", request.AccountId);

    // TODO: Get account from account service.
    await Task.Delay(100); // Simulate async work.

    // Send a dummy response for demonstration purposes
    var response = new GetAccountResponse {
      Account = new Account {
        AccountId = request.AccountId,
        Username = "dummy_user",
        CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
        UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
      }
    };

    return response;
  }

  [AllowAnonymous]
  public override Task<Empty> Create(CreateAccountRequest request, ServerCallContext context) {
    return base.Create(request, context);
  }

  [Authorize(Policy = "RequireGeneral")]
  public override Task<UpdateAccountResponse> Update(UpdateAccountRequest request, ServerCallContext context) {
    return base.Update(request, context);
  }

  [Authorize(Policy = "RequireGeneral")]
  public override Task<Empty> Delete(DeleteAccountRequest request, ServerCallContext context) {
    return base.Delete(request, context);
  }
}
