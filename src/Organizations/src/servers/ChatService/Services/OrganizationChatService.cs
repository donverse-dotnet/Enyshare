using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Chats.Models;

namespace Pocco.Svc.ChatService.Services;

public class OrganizationChatService : V0OrganizationChatService.V0OrganizationChatServiceBase {
  private readonly IChatRepository _repository;

  public OrganizationChatService(IChatRepository repository) {
    _repository = repository;
  }


  public override async Task<Empty> Create(V0CreateRequest request, ServerCallContext context) {

    var chat = new Chat {
      Name = request.Name,
      Description = context.RequestHeaders.GetValue("discription") ?? "",
      Is_Private = false,
      Created_At = DateTime.UtcNow
    };
    var created = await _repository.CreateAsync(request.OrgId, chat);
    return new Empty();
  }

  public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
    var updateChat = new Chat {
      Name = request.Chatsmodel.Name,
      Description = request.Chatsmodel.Description,
      Is_Private = request.Chatsmodel.IsPrivate
    };

    var updated = await _repository.UpdateAsync(request.Chatsmodel.OrgId, updateChat);
    if (updated == null) {
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

  private V0ChatsModel ToReply(Chat chat) {
    var chatModel = new V0ChatsModel {
      Id = chat.Id,
      OrgId = chat.Org_Id,
      Name = chat.Name,
      Description = chat.Description,
      CreatedBy = chat.Created_By,
      CreatedAt = Timestamp.FromDateTime(chat.Created_At.ToUniversalTime()),
      IsPrivate = false
    };

    chatModel.MemberIds.AddRange(chat.Member_Ids);

    return chatModel;
  }
}
