using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl : V0ApiService.V0ApiServiceBase {
  public ApiServiceImpl([FromServices] ILogger<ApiServiceImpl> logger) {
    _logger = logger;

    _logger.LogInformation("ApiServiceImpl initialized.");
  }

  private readonly ILogger<ApiServiceImpl> _logger;
}
