using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  // org.InfosService.InfosServiceClient <- コンストラクタで受け取れるようにする

  // List
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<V0ListOrganizationsResponse> List(Empty request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.InfosService.InfosServiceClientを使ったロジックを実装

    // 例として、空のレスポンスを返す
    var response = new V0ListOrganizationsResponse();
    response.Organizations.Add(new Organization {
      OrganizationId = "org-123",
      Name = "Example Organization",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    response.Organizations.Add(new Organization {
      OrganizationId = "org-456",
      Name = "Another Organization",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    return await Task.FromResult(response);
  }

  // Get
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Organization> Get(V0BaseRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.InfosService.InfosServiceClientを使ったロジックを実装

    // 例として、ダミーの組織情報を返す
    var organization = new Organization {
      OrganizationId = request.Id,
      Name = "Example Organization",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    };
    return await Task.FromResult(organization);
  }

  // Create
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> Create(V0CreateXRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.InfosService.InfosServiceClientを使ったロジックを実装
    // 成功かどうかはイベントで受け取る想定

    return await Task.FromResult(new Empty());
  }

  // Update -> name
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> UpdateOrganizationName(V0UpdateOrganizationNameRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.InfosService.InfosServiceClientを使ったロジックを実装
    // 名前の変更はオーナーのみ可能
    // 成功かどうかはイベントで受け取る想定

    return await Task.FromResult(new Empty());
  }

  // Delete
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> Delete(V0BaseRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.InfosService.InfosServiceClientを使ったロジックを実装
    // 組織の削除はオーナーのみ可能
    // 成功かどうかはイベントで受け取る想定

    return await Task.FromResult(new Empty());
  }
}
