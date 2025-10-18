using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Chats.Models;

using SharpCompress.Common;

namespace Pocco.Svc.ChatService.Services;

public class OrganizationChatService : V0OrganizationChatService.V0OrganizationChatServiceBase {
  private readonly IChatRepository _repository;
  private readonly ILogger<OrganizationChatService> _logger;

  public OrganizationChatService([FromServices] IChatRepository repository, [FromServices] ILogger<OrganizationChatService> logger) {
    _repository = repository;
    _logger = logger;

    _logger.LogInformation("OrganizationChatService is initialized!");
  }


  public override async Task<Empty> Create(V0CreateRequest request, ServerCallContext context) {

    var chat = new Chat {
      Id = ObjectId.GenerateNewId().ToString(),
      Name = request.Name,
      Description = "",     //作成するときは空のまま
      IsPrivate = false,
      CreatedAt = DateTime.UtcNow
    };
    Chat createdChat = await _repository.CreateAsync(request.OrgId, chat);
    _logger.LogInformation("{ChatId} is successfully created on {OrgId}", createdChat.Id, request.OrgId);
    return new Empty();
  }

  public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
    var updateChat = new Chat {
      Id = request.Chatsmodel.Id,
      Name = request.Chatsmodel.Name,
      Description = request.Chatsmodel.Description,
      IsPrivate = request.Chatsmodel.IsPrivate
    };

    var updated = await _repository.TryUpdateAsync(request.Chatsmodel.OrgId, request.Chatsmodel.Id, updateChat);
    if (updated == false) {
      throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
    }

    return new Empty();
  }

  public override async Task<Empty> Get(V0GetRequest request, ServerCallContext context) {
    var chat = await _repository.GetByIdAsync(request.OrgId, request.Id);
    if (chat == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
    }
    return new Empty();
  }

  public override async Task<Empty> Delete(V0DeleteRequest request, ServerCallContext context) {
    var success = await _repository.DeleteAsync(request.OrgId, request.Id);
    if (!success) {
      throw new RpcException(new Status(StatusCode.NotFound, "Chat not found or no fields to delete"));
    }
    return new Empty();
  }
}
