using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl : V0ApiService.V0ApiServiceBase {
  public ApiServiceImpl(
    [FromServices] V0AccountService.V0AccountServiceClient asc,
    [FromServices] ILogger<ApiServiceImpl> logger
  ) {
    _accountServiceClient = asc;
    _logger = logger;

    _logger.LogInformation("ApiServiceImpl initialized.");
  }

  private readonly ILogger<ApiServiceImpl> _logger;
}
