using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  private readonly ILogger<OrganizationsServiceImpl> _logger;

  public OrganizationsServiceImpl(ILogger<OrganizationsServiceImpl> logger) {
    _logger = logger;

    _logger.LogInformation("OrganizationsServiceImpl initialized");
  }
}
