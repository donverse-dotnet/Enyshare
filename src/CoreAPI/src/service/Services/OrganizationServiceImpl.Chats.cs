using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  // org.ChatsService.ChatsServiceClient <- コンストラクタで受け取れるようにする

  // List
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<V0ListChatsResponse> ListChats(V0ListXRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.ChatsService.ChatsServiceClientを使ったロジックを実装
    // 例として、空のレスポンスを返す
    var response = new V0ListChatsResponse();
    response.Chats.Add(new Chat {
      ChatId = "chat-123",
      Name = "General Chat",
      Description = "This is the general chat for the organization.",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    response.Chats.Add(new Chat {
      ChatId = "chat-456",
      Name = "Project Chat",
      Description = "Chat for project discussions.",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    return await Task.FromResult(response);
  }

  // Get
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Chat> GetChat(V0BaseRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.ChatsService.ChatsServiceClientを使ったロジックを実装

    // 例として、ダミーのチャット情報を返す
    var chat = new Chat {
      ChatId = request.Id,
      Name = "General Chat",
      Description = "This is the general chat for the organization.",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    };
    return await Task.FromResult(chat);
  }

  // Create
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> CreateChat(V0CreateChatRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.ChatsService.ChatsServiceClientを使ったロジックを実装
    // チャンネル作成権限を持つかどうかのチェックも行う

    // 成功したかどうかはイベントで受け取る想定なので、ここでは空のレスポンスを返す
    return await Task.FromResult(new Empty());
  }

  // Update
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> UpdateChat(V0UpdateChatRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.ChatsService.ChatsServiceClientを使ったロジックを実装
    // チャンネル更新権限を持つかどうかのチェックも行う

    // 成功したかどうかはイベントで受け取る想定なので、ここでは空のレスポンスを返す
    return await Task.FromResult(new Empty());
  }

  // Delete
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> DeleteChat(V0BaseRequest request, ServerCallContext context) {
    // ユーザーIDをコンテキストから取得
    var userId = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User ID is missing in the token."));
    }

    // ここにorg.ChatsService.ChatsServiceClientを使ったロジックを実装
    // チャンネル削除権限を持つかどうかのチェックも行う

    // 成功したかどうかはイベントで受け取る想定なので、ここでは空のレスポンスを返す
    return await Task.FromResult(new Empty());
  }
}
