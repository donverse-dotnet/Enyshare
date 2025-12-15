using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;

using Pocco.Libs.Protobufs.EventBridge.Services;
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

    return createdChat.ToV0ChatModel();
  }
}
