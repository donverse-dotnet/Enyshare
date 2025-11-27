using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.Auth.Services;
using Pocco.Libs.Protobufs.CoreAPI.Services;
using Pocco.Libs.Protobufs.Organizations_Info.Services;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl : V0ApiService.V0ApiServiceBase {
  public ApiServiceImpl(
    [FromServices] V0AccountService.V0AccountServiceClient asc,
    [FromServices] V0AuthService.V0AuthServiceClient authsc,
    [FromServices] V0OrganizationInfoService.V0OrganizationInfoServiceClient oisc,
    [FromServices] ILogger<ApiServiceImpl> logger
  ) {
    _logger = logger;

    _accountServiceClient = asc;
    _authServiceClient = authsc;
    _orgInfoService = oisc;

    _logger.LogInformation("ApiServiceImpl initialized.");
  }

  private readonly ILogger<ApiServiceImpl> _logger;
}
