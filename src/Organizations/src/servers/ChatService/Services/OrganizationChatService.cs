using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Chats.Models;
using Pocco.Svc.EventBridge.Protobufs.Enums;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;

using SharpCompress.Common;

namespace Pocco.Svc.ChatService.Services;

public class OrganizationChatService : V0OrganizationChatService.V0OrganizationChatServiceBase {
  private readonly IChatRepository _repository;
  private readonly ILogger<OrganizationChatService> _logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;

  public OrganizationChatService([FromServices] IChatRepository repository, [FromServices] ILogger<OrganizationChatService> logger, [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge) {
    _repository = repository;
    _logger = logger;
    _eventBridge = eventBridge;

    _logger.LogInformation("OrganizationChatService is initialized!");
  }

  public override async Task<V0GetListReply> GetList(V0GetListRequest request, ServerCallContext context) {
    var chats = await _repository.GetListAsync(request.OrgId);

    var reply = new V0GetListReply();

    reply.Chatsmodel.AddRange(chats.Select(c => new V0ChatsModel {
      Id = c.Id
    }));
    return reply;
  }

  public override async Task<V0ChatChangesReply> Create(V0CreateRequest request, ServerCallContext context) {

    var chat = new Chat {
      Id = ObjectId.GenerateNewId().ToString(),
      OrgId = request.OrgId,
      Name = request.Name,
      Description = "",     //作成するときは空のまま
      CreatedBy = request.CreatedBy,
      IsPrivate = false,
      CreatedAt = DateTime.UtcNow
    };
    Chat createdChat = await _repository.CreateAsync(request.OrgId, chat);
    _logger.LogInformation("{ChatId} is successfully created on {OrgId}", createdChat.Id, request.OrgId);

    //イベントを伝搬させるのをEventBridgeに依頼

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnChatCreated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrgId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{createdChat.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{createdChat.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{createdChat.Description}" });
    newEventData.Payload.Fields.Add("created_by", new Value { StringValue = $"{createdChat.CreatedBy}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{createdChat.CreatedAt}" });
    newEventData.Payload.Fields.Add("is_private", new Value { StringValue = $"{createdChat.IsPrivate}" });

    var createdEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0ChatChangesReply {
      EventId = createdEventData.EventId
    };
  }

  public override async Task<V0ChatChangesReply> Update(V0UpdateRequest request, ServerCallContext context) {
    var updateChat = new Chat {
      Id = request.Chatsmodel.Id,
      OrgId = request.Chatsmodel.OrgId,
      Name = request.Chatsmodel.Name,
      Description = request.Chatsmodel.Description,
      CreatedBy = request.Chatsmodel.CreatedBy,
      IsPrivate = request.Chatsmodel.IsPrivate
    };

    var updated = await _repository.TryUpdateAsync(request.Chatsmodel.OrgId, request.Chatsmodel.Id, updateChat);
    if (updated == false) {
      throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnChatUpdated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.Chatsmodel.OrgId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{updateChat.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{updateChat.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{updateChat.Description}" });
    newEventData.Payload.Fields.Add("created_by", new Value { StringValue = $"{updateChat.CreatedBy}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{updateChat.CreatedAt}" });
    newEventData.Payload.Fields.Add("is_private", new Value { StringValue = $"{updateChat.IsPrivate}" });

    var updatedEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0ChatChangesReply {
      EventId = updatedEventData.EventId
    };
  }

  public override async Task<V0GetReply> Get(V0GetRequest request, ServerCallContext context) {
    var chat = await _repository.GetByIdAsync(request.OrgId, request.Id);
    if (chat == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
    }
    return new V0GetReply {
      Chatsmodel = chat.ToV0ChatModel()
    };
  }

  public override async Task<V0ChatChangesReply> Delete(V0DeleteRequest request, ServerCallContext context) {
    var success = await _repository.DeleteAsync(request.OrgId, request.Id);
    if (!success) {
      throw new RpcException(new Status(StatusCode.NotFound, "Chat not found or no fields to delete"));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnChatDeleted",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrgId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{request.Id}" });

    var deletedEventData = _eventBridge.NewEvent(
     newEventData
   );

    return new V0ChatChangesReply {
      EventId = deletedEventData.EventId
    };
  }
}
