using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl {
  private readonly V0OrganizationInfoService.V0OrganizationInfoServiceClient _orgInfoService;

  #region Informations
  // public override Task<V0ListOrganizationsResponse> List(Empty request, ServerCallContext context) {
  //   return base.List(request, context);
  // }

  public override async Task<Organization> Get(V0BaseRequest request, ServerCallContext context) {
    var organization = await _orgInfoService.GetInfoAsync(new V0GetInfoOrganizationRequest { Id = request.Id });

    return new Organization {
      OrganizationId = organization.Id,
      Name = organization.Name,
      Description = organization.Description,
      CreatedBy = organization.CreatedBy,
      CreatedAt = organization.CreatedAt,
      UpdatedAt = organization.UpdatedAt
    };
  }

  public override async Task<V0EventInvokedResponse> Create(Libs.Protobufs.Services.V0CreateOrganizationRequest request, ServerCallContext context) {
    var reply = await _orgInfoService.CreateAsync(new Libs.Protobufs.Types.V0CreateOrganizationRequest { // TODO: 名前空間の整理
      Name = request.Base.Name,
      Description = request.Description
    });
    return new V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<V0EventInvokedResponse> UpdateOrganization(Libs.Protobufs.Services.V0UpdateOrganizationRequest request, ServerCallContext context) {
    var reply = await _orgInfoService.UpdateAsync(new Libs.Protobufs.Types.V0UpdateOrganizationRequest {
      Id = request.OrganizationId,
      Name = request.Name,
      Description = request.Description
    });

    return new V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<V0EventInvokedResponse> Delete(V0BaseRequest request, ServerCallContext context) {
    var reply = await _orgInfoService.DeleteAsync(new V0DeleteOrganizationRequest {
      Id = request.Id
    });
    return new V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }
  #endregion

  #region Members
  public override async Task<V0ListMembersResponse> ListMembers(V0ListXRequest request, ServerCallContext context) {
    // var members = await _organizationService.ListOrganizationMembersAsync(request.Id);
    var members = new List<Libs.Protobufs.Services.Member>
    {
      new Libs.Protobufs.Services.Member { },
      new Libs.Protobufs.Services.Member { }
    };

    var response = new V0ListMembersResponse();
    response.Members.AddRange(members);
    return response;
  }

  public override async Task<Libs.Protobufs.Services.Member> GetMember(V0BaseRequest request, ServerCallContext context) {
    // var member = await _organizationService.GetOrganizationMemberByIdAsync(request.Id);
    var member = new Libs.Protobufs.Services.Member { }; // TODO: Remove mock
    return member;
  }

  public override async Task<V0EventInvokedResponse> JoinMember(V0JoinMemberRequest request, ServerCallContext context) {
    // await _organizationService.AddMemberToOrganizationAsync(request.OrganizationId, request.UserId, request.Role);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> LeaveMember(V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.RemoveMemberFromOrganizationAsync(request.Id);
    return new V0EventInvokedResponse();
  }

  // TODO: ModerateMember (ban, kick, etc.)
  // TODO: UpdateMemberInfo (role, status, etc.)
  #endregion

  #region Roles
  public override Task<V0ListRolesResponse> ListRoles(V0ListXRequest request, ServerCallContext context) {
    // var roles = await _organizationService.ListOrganizationRolesAsync(request.Id);
    var roles = new List<Role>
    {
      new Role { },
      new Role { }
    }; // TODO: Remove mock

    var response = new V0ListRolesResponse();
    response.Roles.AddRange(roles);
    return Task.FromResult(response);
  }

  public override async Task<Role> GetRole(V0BaseRequest request, ServerCallContext context) {
    // var role = await _organizationService.GetOrganizationRoleByIdAsync(request.Id);
    var role = new Role { }; // TODO: Remove mock
    return role;
  }

  public override async Task<V0EventInvokedResponse> CreateRole(V0CreateXRequest request, ServerCallContext context) {
    // await _organizationService.CreateOrganizationRoleAsync(request.Name, request.Permissions);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> UpdateRole(Role request, ServerCallContext context) {
    // await _organizationService.UpdateOrganizationRoleAsync(request.Id, request.Name, request.Permissions);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> DeleteRole(V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.DeleteOrganizationRoleAsync(request.Id);
    return new V0EventInvokedResponse();
  }
  #endregion

  #region Chats
  public override async Task<V0ListChatsResponse> ListChats(V0ListXRequest request, ServerCallContext context) {
    // var chats = await _organizationService.ListOrganizationChatsAsync(request.Id);
    var chats = new List<Chat>
    {
      new Chat { },
      new Chat { }
    }; // TODO: Remove mock

    var response = new V0ListChatsResponse();
    response.Chats.AddRange(chats);
    return response;
  }

  public override async Task<Chat> GetChat(V0BaseRequest request, ServerCallContext context) {
    // var chat = await _organizationService.GetOrganizationChatByIdAsync(request.Id);
    var chat = new Chat { }; // TODO: Remove mock
    return chat;
  }

  public override async Task<V0EventInvokedResponse> CreateChat(V0CreateChatRequest request, ServerCallContext context) {
    // await _organizationService.CreateOrganizationChatAsync(request.Name, request.Description);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> UpdateChat(V0UpdateChatRequest request, ServerCallContext context) {
    // await _organizationService.UpdateOrganizationChatAsync(request.Id, request.Name, request.Description);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> DeleteChat(V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.DeleteOrganizationChatAsync(request.Id);
    return new V0EventInvokedResponse();
  }
  #endregion

  #region Messages
  public override async Task<V0ListMessagesResponse> ListMessages(V0ListMessagesRequest request, ServerCallContext context) {
    // var messages = await _organizationService.ListChatMessagesAsync(request.Id);
    var messages = new List<Message> {
      new Message { },
      new Message { }
    };

    var response = new V0ListMessagesResponse();
    response.Messages.AddRange(messages);
    return response;
  }

  public override async Task<Message> GetMessage(V0BaseRequest request, ServerCallContext context) {
    // var message = await _organizationService.GetChatMessageByIdAsync(request.Id);
    var message = new Message { }; // TODO: Remove mock
    return message;
  }

  public override async Task<V0EventInvokedResponse> CreateMessage(Message request, ServerCallContext context) {
    // await _organizationService.SendMessageToChatAsync(request.ChatId, request.SenderId, request.Content);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> UpdateMessage(Message request, ServerCallContext context) {
    // await _organizationService.UpdateChatMessageAsync(request.Id, request.Content);
    return new V0EventInvokedResponse();
  }

  public override async Task<V0EventInvokedResponse> DeleteMessage(V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.DeleteChatMessageAsync(request.Id);
    return new V0EventInvokedResponse();
  }
  #endregion
}
