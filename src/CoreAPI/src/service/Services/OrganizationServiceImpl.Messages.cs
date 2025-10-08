using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Services;

public partial class OrganizationsServiceImpl : V0OrganizationsService.V0OrganizationsServiceBase {
  // org.MessagesService.MessagesServiceClient <- コンストラクタで受け取れるようにする

  // ListMessages
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<V0ListMessagesResponse> ListMessages(V0ListMessagesRequest request, ServerCallContext context) {
    // ここにorg.MessagesService.MessagesServiceClientを使ったロジックを実装
    // 例として、空のレスポンスを返す
    var response = new V0ListMessagesResponse();
    response.Messages.Add(new Message {
      MessageId = "msg-123",
      ChatId = request.ChatId,
      SenderId = "user-456",
      Content = "Hello, this is a test message.",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    response.Messages.Add(new Message {
      MessageId = "msg-789",
      ChatId = request.ChatId,
      SenderId = "user-789",
      Content = "This is another test message.",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    });
    return await Task.FromResult(response);
  }

  // GetMessage
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Message> GetMessage(V0BaseRequest request, ServerCallContext context) {
    // ここにorg.MessagesService.MessagesServiceClientを使ったロジックを実装
    // 例として、ダミーのメッセージ情報を返す
    var message = new Message {
      MessageId = request.Id,
      ChatId = "chat-123",
      SenderId = "user-456",
      Content = "Hello, this is a test message.",
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    };
    return await Task.FromResult(message);
  }

  // SendMessage (CreateMessage)
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> CreateMessage(Message request, ServerCallContext context) {
    // ここにorg.MessagesService.MessagesServiceClientを使ったロジックを実装
    // 例として、成功した場合は空のレスポンスを返す
    return await Task.FromResult(new Empty());
  }
  // UpdateMessage
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> UpdateMessage(Message request, ServerCallContext context) {
    // ここにorg.MessagesService.MessagesServiceClientを使ったロジックを実装
    // 例として、成功した場合は空のレスポンスを返す
    return await Task.FromResult(new Empty());
  }
  // DeleteMessage
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> DeleteMessage(V0BaseRequest request, ServerCallContext context) {
    // ここにorg.MessagesService.MessagesServiceClientを使ったロジックを実装
    // 例として、成功した場合は空のレスポンスを返す
    return await Task.FromResult(new Empty());
  }

  // AddReaction
  // RemoveReaction
}
