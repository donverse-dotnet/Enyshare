using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  // org.MembersService.MembersServiceClient <- コンストラクタで受け取れるようにする

  // Request to join
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> JoinMember(V0JoinMemberRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.MembersService.MembersServiceClientを使ったロジックを実装
    // コード検証→参加処理

    // 参加できた場合は、OKを返す
    return await Task.FromResult(new Empty());
  }

  // Leave from organization
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> LeaveMember(V0BaseRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }
    // ここにorg.MembersService.MembersServiceClientを使ったロジックを実装
    // 退会処理

    // 退会できた場合は、OKを返す
    return await Task.FromResult(new Empty());
  }

  // Create invite code // Not supported yet
  // List invite codes // Not supported yet
  // Revoke invite code // Not supported yet

  // List members
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<V0ListMembersResponse> ListMembers(V0ListXRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.MembersService.MembersServiceClientを使ったロジックを実装
    // 例として、空のレスポンスを返す
    var response = new V0ListMembersResponse();
    response.Members.Add(new Member {
      MemberId = "member-123",
      UserId = "user-123",
      Role = "Admin",
      JoinedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    response.Members.Add(new Member {
      MemberId = "member-456",
      UserId = "user-456",
      Role = "Member",
      JoinedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    return await Task.FromResult(response);
  }

  // Get member
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Member> GetMember(V0BaseRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.MembersService.MembersServiceClientを使ったロジックを実装

    // 例として、ダミーのメンバー情報を返す
    var member = new Member {
      MemberId = request.Id,
      UserId = "user-123",
      Role = "Admin",
      JoinedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    };
    return await Task.FromResult(member);
  }

  // Warn member
  // Unwarn member
  // Timeout member
  // Kick member
  // Ban member
  // Unban member
}
