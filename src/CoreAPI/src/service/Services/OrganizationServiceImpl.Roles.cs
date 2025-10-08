using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  // org.RolesService.RolesServiceClient <- コンストラクタで受け取れるようにする

  // ListRoles
  public override async Task<V0ListRolesResponse> ListRoles(V0ListXRequest request, ServerCallContext context) {
    // ここにorg.RolesService.RolesServiceClientを使ったロジックを実装
    // 例として、空のレスポンスを返す
    var response = new V0ListRolesResponse();
    // response.Roles.Add(...); // 必要に応じてロールを追加
    return await Task.FromResult(response);
  }

  // GetRole
  public override async Task<Role> GetRole(V0BaseRequest request, ServerCallContext context) {
    // ここにorg.RolesService.RolesServiceClientを使ったロジックを実装
    // 例として、ダミーのロール情報を返す
    var role = new Role();

    return await Task.FromResult(role);
  }

  // CreateRole
  public override Task<Empty> CreateRole(V0CreateXRequest request, ServerCallContext context) {
    // ここにorg.RolesService.RolesServiceClientを使ったロジックを実装

    return Task.FromResult(new Empty());
  }

  // UpdateRole
  public override Task<Empty> UpdateRole(Role request, ServerCallContext context) {
    // ここにorg.RolesService.RolesServiceClientを使ったロジックを実装

    return Task.FromResult(new Empty());
  }

  // DeleteRole
  public override Task<Empty> DeleteRole(V0BaseRequest request, ServerCallContext context) {
    // ここにorg.RolesService.RolesServiceClientを使ったロジックを実装

    return Task.FromResult(new Empty());
  }
}
