using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  public override Task<GetOrganizationResponse> Get(GetOrganizationRequest request, ServerCallContext context) {
    // TODO: Get organization data from service.

    // Send default response
    return base.Get(request, context);
  }

  public override Task<ListOrganizationsResponse> List(Empty request, ServerCallContext context) {
    return base.List(request, context);
  }

  // roles
  // members
  // chats
}
