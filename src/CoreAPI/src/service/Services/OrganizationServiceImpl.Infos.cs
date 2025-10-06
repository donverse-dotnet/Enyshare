using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  // Create
  // Get
  // List -> ユーザーが初回ロード時などに組織一覧を取得するために使用
  // Update
  // Delete
}
