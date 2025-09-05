using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Chats.Models;

namespace Pocco.Svc.Chats.Services;

public class OrganizationChatService : V0OrganizationChatService.V0OrganizationChatServiceBase {
    private readonly IChatRepository _repository;

    public OrganizationChatService(IChatRepository repository) {
        _repository = repository;
    }

    public override async Task<V0CreateReply> Create(V0CreateRequest request, ServerCallContext context) {

        var chat = new Chat {
            Name = request.Name,
            Description = Console.ReadLine()?.Trim(),
            Created_By = ,
            Created_At = DateTime.UtcNow
        };
        var result = await _repository.CreateAsync(chat);
        return
    }

    public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
        var updated = await _repository.UpdateAsync(
            request.Chatsmodel.Name,
            Description
            Is_private
        );

        if (updated == null) {
            throw new RpcException(new Status(StatusCode.NotFound, "Chat not found"));
        }

        return ToReply(updated);
    }

    public override async Task<V0GetReply> Get(V0GetRequest request, ServerCallContext context) {
        var chat = await _repository.GetByIdAsync(request.OrgId, request.Id);

        return ToReply(chat);
    }

    public override async Task<V0DeleteReply> Delete(V0DeleteRequest request, ServerCallContext context) {
        var success = await _repository.DeleteAsync(request.OrgId, request.Id);
        return new V0DeleteReply { Success = success };
    }

    private static Chat ToReply(Chat chat) => new Chat {
        Id = chat.Id,
        Org_Id = chat.Org_Id,
        Name = chat.Name,
        Description = chat.Description,
        Created_By = chat.Created_By,
        Created_At = Timestamp.FromDateTime(chat.Created_At.ToUniversalTime()),
        Is_Private = false,
        Member_Ids = { }
    };
}