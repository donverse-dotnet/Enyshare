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

        }
    }
}