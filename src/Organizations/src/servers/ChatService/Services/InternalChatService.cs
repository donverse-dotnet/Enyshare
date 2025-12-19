using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;

using Pocco.Libs.Protobufs.EventBridge.Enums;
using Pocco.Libs.Protobufs.EventBridge.Services;
using Pocco.Libs.Protobufs.EventBridge.Types;
using Pocco.Libs.Protobufs.Organizations_Chat.Services;
using Pocco.Libs.Protobufs.Organizations_Chat.Types;
using Pocco.Svc.Chats.Models;

namespace Pocco.Svc.ChatService.Services;

public class InternalChatServiceImpl : V0InternalOrganizationChatService.V0InternalOrganizationChatServiceBase {
  private readonly IChatRepository _repository;
  private readonly ILogger<InternalChatServiceImpl> _logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;

  public InternalChatServiceImpl(
    [FromServices] IChatRepository repository,
    [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge,
    [FromServices] ILogger<InternalChatServiceImpl> logger
  ) {
    _repository = repository;
    _eventBridge = eventBridge;
    _logger = logger;

    _logger.LogInformation("InternalChatServiceImpl is initialized!");
  }

  // デフォルトチャットを作成する
  public override async Task<V0ChatsModel> Create(V0CreateRequest request, ServerCallContext context) {
    var currentTime = DateTime.UtcNow;

    var chat = new Chat {
      Id = ObjectId.GenerateNewId().ToString(),
      OrgId = request.OrgId,
      Name = request.Name,  // Generalに絶対する
      Description = "",     //作成するときは空のまま
      CreatedBy = request.CreatedBy,
      IsPrivate = false,
      CreatedAt = currentTime,
      UpdatedAt = currentTime
    };
    var createdChat = await _repository.CreateAsync(request.OrgId, chat);
    _logger.LogInformation("{ChatId} is successfully created on {OrgId}", createdChat.Id, request.OrgId);

    //イベントを伝搬させるのをEventBridgeに依頼
    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationChatCreated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };
    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrgId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{createdChat.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{createdChat.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{createdChat.Description}" });
    newEventData.Payload.Fields.Add("created_by", new Value { StringValue = $"{createdChat.CreatedBy}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{createdChat.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{createdChat.UpdatedAt}" });
    newEventData.Payload.Fields.Add("is_private", new Value { StringValue = $"{createdChat.IsPrivate}" });

    await _eventBridge.NewEventAsync(newEventData);

    return createdChat.ToV0ChatModel();
  }
}
