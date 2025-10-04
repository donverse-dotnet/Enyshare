using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  private readonly ILogger<OrganizationsServiceImpl> _logger;

  public OrganizationsServiceImpl(ILogger<OrganizationsServiceImpl> logger) {
    _logger = logger;

    _logger.LogInformation("OrganizationsServiceImpl initialized");
  }

  #region 参照系
  public override Task<GetOrganizationResponse> Get(GetOrganizationRequest request, ServerCallContext context) {
    // TODO: Get organization data from service.

    // Send default response
    return base.Get(request, context);
  }

  public override Task<ListOrganizationsResponse> List(Empty request, ServerCallContext context) {
    return base.List(request, context);
  }
  #endregion

  #region 更新系
  public override Task<Empty> Create(CreateOrganizationRequest request, ServerCallContext context) {
    return base.Create(request, context);
  }

  public override Task<Empty> UpdateOrganizationName(UpdateOrganizationNameRequest request, ServerCallContext context) {
    return base.UpdateOrganizationName(request, context);
  }

  public override Task<Empty> Delete(DeleteOrganizationRequest request, ServerCallContext context) {
    return base.Delete(request, context);
  }
  #endregion
}
