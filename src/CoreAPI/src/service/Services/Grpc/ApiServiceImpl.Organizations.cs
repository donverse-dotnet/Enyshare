using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl {
  #region Informations
  public override async Task<V0ListOrganizationsResponse> List(Empty request, ServerCallContext context) {
    // var organizations = await _organizationService.ListOrganizationsAsync();
    var organizations = new List<Organization>
    {
      new Organization { },
      new Organization { }
    }; // TODO: Remove mock
    var response = new V0ListOrganizationsResponse();
    response.Organizations.AddRange(organizations);
    return response;
  }

  public override async Task<Organization> Get(V0BaseRequest request, ServerCallContext context) {
    // var organization = await _organizationService.GetOrganizationByIdAsync(request.Id);
    var organization = new Organization { }; // TODO: Remove mock
    return organization;
  }

  public override async Task<V0EventInvokedResponse> Create(V0CreateXRequest request, ServerCallContext context) {
    // await _organizationService.CreateOrganizationAsync(request.Name, request.Description);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> UpdateOrganizationName(V0UpdateOrganizationNameRequest request, ServerCallContext context) {
    // await _organizationService.UpdateOrganizationNameAsync(request.Id, request.NewName);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> Delete(V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.DeleteOrganizationAsync(request.Id);
    return new V0EventInvokedResponse();
  }
  #endregion
}
