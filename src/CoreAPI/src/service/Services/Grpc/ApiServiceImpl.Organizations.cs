using Grpc.Core;
using CoreAPI_Service = Pocco.Libs.Protobufs.CoreAPI.Services;
using Pocco.Libs.Protobufs.Organizations_Info.Services;
using Pocco.Libs.Protobufs.Organizations_Info.Types;
using Pocco.Libs.Protobufs.Organizations_Member.Services;
using Pocco.Libs.Protobufs.Organizations_Member.Types;
using Pocco.Libs.Protobufs.Organizations_Chat.Services;
using Pocco.Libs.Protobufs.Organizations_Message.Services;
using Pocco.Libs.Protobufs.Organizations_Role.Services;
using Pocco.Libs.Protobufs.Organizations_Role.Types;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl {
  private readonly V0OrganizationInfoService.V0OrganizationInfoServiceClient _orgInfoService;
  private readonly V0OrganizationMemberService.V0OrganizationMemberServiceClient _orgMemberService;
  private readonly V0OrganizationChatService.V0OrganizationChatServiceClient _orgChatService;
  private readonly OrganizationMessageRpcService.OrganizationMessageRpcServiceClient _orgMessageService;
  private readonly V0RoleService.V0RoleServiceClient _orgRoleService;

  #region Informations
  public override async Task<CoreAPI_Service.Organization> Get(CoreAPI_Service.V0GetXRequest request, ServerCallContext context) {
    var organization = await _orgInfoService.GetInfoAsync(new V0GetInfoOrganizationRequest { Id = request.Id });

    return new CoreAPI_Service.Organization {
      OrganizationId = organization.Id,
      Name = organization.Name,
      Description = organization.Description,
      CreatedBy = organization.CreatedBy,
      CreatedAt = organization.CreatedAt,
      UpdatedAt = organization.UpdatedAt
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> Create(CoreAPI_Service.V0CreateOrganizationRequest request, ServerCallContext context) {
    var reply = await _orgInfoService.CreateAsync(new V0CreateOrganizationRequest { // TODO: 名前空間の整理
      Name = request.Base.Name,
      Description = request.Description
    });
    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateOrganization(CoreAPI_Service.V0UpdateOrganizationRequest request, ServerCallContext context) {
    var reply = await _orgInfoService.UpdateAsync(new V0UpdateOrganizationRequest {
      Id = request.OrganizationId,
      Name = request.Name,
      Description = request.Description
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> Delete(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
    var reply = await _orgInfoService.DeleteAsync(new V0DeleteOrganizationRequest {
      Id = request.Id
    });
    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }
  #endregion

  #region Members
  public override async Task<CoreAPI_Service.V0ListMembersResponse> ListMembers(CoreAPI_Service.V0ListXRequest request, ServerCallContext context) {
    var reply = await _orgMemberService.ListAsync(new Libs.Protobufs.Organizations_Member.Types.V0GetListRequest {
      OrganizationId = request.Base.Id
    });

    var response = new CoreAPI_Service.V0ListMembersResponse();

    foreach (var member in reply.Members) {
      var m = new CoreAPI_Service.Member {
        MemberId = member.Id,
        JoinedAt = member.JoinedAt
      };
      m.Roles.AddRange(member.Roles);

      response.Members.Add(m);
    }

    return response;
  }

  public override async Task<CoreAPI_Service.Member> GetMember(CoreAPI_Service.V0GetXRequest request, ServerCallContext context) {
    var reply = await _orgMemberService.GetAsync(new Libs.Protobufs.Organizations_Member.Types.V0GetRequest {
      MemberId = request.Id,
      OrganizationId = request.OrganizationId
    });

    var member = new CoreAPI_Service.Member {
      MemberId = reply.Id,
      JoinedAt = reply.JoinedAt
    };
    member.Roles.AddRange(reply.Roles);

    return member;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> JoinMember(CoreAPI_Service.V0JoinMemberRequest request, ServerCallContext context) {
    var reply = await _orgMemberService.JoinAsync(new V0JoinRequest {
      UserId = request.UserId,
      OrganizationId = request.OrganizationId,
      Code = request.InviteCode,
      InvokedBy = request.UserId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> LeaveMember(CoreAPI_Service.V0LeaveMemberRequest request, ServerCallContext context) {
    var reply = await _orgMemberService.LeaveAsync(new V0LeaveRequest {
      MemberId = request.UserId,
      OrganizationId = request.OrganizationId,
      InvokedBy = request.UserId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  // TODO: ModerateMember (ban, kick, etc.)
  // TODO: UpdateMemberInfo (role, status, etc.)
  #endregion

  #region Roles
  public override async Task<CoreAPI_Service.V0ListRolesResponse> ListRoles(CoreAPI_Service.V0ListXRequest request, ServerCallContext context) {
    var reply = await _orgRoleService.GetListAsync(new Libs.Protobufs.Organizations_Role.Types.V0GetListRequest {
      OrgId = request.Base.Id
    });

    var response = new CoreAPI_Service.V0ListRolesResponse();

    foreach (var role in reply.Roles) {
      var r = new CoreAPI_Service.Role {
        RoleId = role.Id,
        Name = role.Name,
        CreatedAt = role.CreatedAt,
        UpdatedAt = role.UpdatedAt
      };
      r.Permissions.AddRange(role.Permissions);

      response.Roles.Add(r);
    }

    return response;
  }

  public override async Task<CoreAPI_Service.Role> GetRole(CoreAPI_Service.V0GetXRequest request, ServerCallContext context) {
    var reply = await _orgRoleService.GetAsync(new Libs.Protobufs.Organizations_Role.Types.V0GetRequest {
      Id = request.Id,
      OrgId = request.OrganizationId
    });

    var role = new CoreAPI_Service.Role {
      RoleId = reply.Rolemodel.Id,
      Name = reply.Rolemodel.Name,
      CreatedAt = reply.Rolemodel.CreatedAt,
      UpdatedAt = reply.Rolemodel.UpdatedAt
    };
    role.Permissions.AddRange(reply.Rolemodel.Permissions);

    return role;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> CreateRole(CoreAPI_Service.V0CreateXRequest request, ServerCallContext context) {
    var reply = await _orgRoleService.CreateAsync(new V0CreateRequest {
      Name = request.Name
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateRole(CoreAPI_Service.Role request, ServerCallContext context) {
    var updateRequest = new Libs.Protobufs.Organizations_Role.Types.V0UpdateRequest {
      Rolemodel = new V0RoleModel {
        Id = request.RoleId,
        OrgId = request.OrganizationId,
        Name = request.Name,
        Permissions = { request.Permissions },
        CreatedAt = request.CreatedAt,
        UpdatedAt = request.UpdatedAt,
      },
      InvokedBy = request.RoleId
    };

    var reply = await _orgRoleService.UpdateAsync(updateRequest);

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> DeleteRole(CoreAPI_Service.V0DeleteXRequest request, ServerCallContext context) {
    var reply = await _orgRoleService.DeleteAsync(new V0DeleteRequest {
      Id = request.Id,
      OrgId = request.OrganizationId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }
  #endregion

  #region Chats
  public override async Task<CoreAPI_Service.V0ListChatsResponse> ListChats(CoreAPI_Service.V0ListXRequest request, ServerCallContext context) {
    // var chats = await _organizationService.ListOrganizationChatsAsync(request.Id);
    var chats = new List<CoreAPI_Service.Chat>
    {
      new CoreAPI_Service.Chat { },
      new CoreAPI_Service.Chat { }
    }; // TODO: Remove mock

    var response = new CoreAPI_Service.V0ListChatsResponse();
    response.Chats.AddRange(chats);
    return response;
  }

  public override async Task<CoreAPI_Service.Chat> GetChat(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
    // var chat = await _organizationService.GetOrganizationChatByIdAsync(request.Id);
    var chat = new CoreAPI_Service.Chat { }; // TODO: Remove mock
    return chat;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> CreateChat(CoreAPI_Service.V0CreateChatRequest request, ServerCallContext context) {
    // await _organizationService.CreateOrganizationChatAsync(request.Name, request.Description);
    return new CoreAPI_Service.V0EventInvokedResponse();
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateChat(CoreAPI_Service.V0UpdateChatRequest request, ServerCallContext context) {
    // await _organizationService.UpdateOrganizationChatAsync(request.Id, request.Name, request.Description);
    return new CoreAPI_Service.V0EventInvokedResponse();
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> DeleteChat(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.DeleteOrganizationChatAsync(request.Id);
    return new CoreAPI_Service.V0EventInvokedResponse();
  }
  #endregion

  #region Messages
  public override async Task<CoreAPI_Service.V0ListMessagesResponse> ListMessages(CoreAPI_Service.V0ListMessagesRequest request, ServerCallContext context) {
    // var messages = await _organizationService.ListChatMessagesAsync(request.Id);
    var messages = new List<CoreAPI_Service.Message> {
      new CoreAPI_Service.Message { },
      new CoreAPI_Service.Message { }
    };

    var response = new CoreAPI_Service.V0ListMessagesResponse();
    response.Messages.AddRange(messages);
    return response;
  }

  public override async Task<CoreAPI_Service.Message> GetMessage(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
    // var message = await _organizationService.GetChatMessageByIdAsync(request.Id);
    var message = new CoreAPI_Service.Message { }; // TODO: Remove mock
    return message;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> CreateMessage(CoreAPI_Service.Message request, ServerCallContext context) {
    // await _organizationService.SendMessageToChatAsync(request.ChatId, request.SenderId, request.Content);
    return new CoreAPI_Service.V0EventInvokedResponse();
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateMessage(CoreAPI_Service.Message request, ServerCallContext context) {
    // await _organizationService.UpdateChatMessageAsync(request.Id, request.Content);
    return new CoreAPI_Service.V0EventInvokedResponse();
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> DeleteMessage(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
    // await _organizationService.DeleteChatMessageAsync(request.Id);
    return new CoreAPI_Service.V0EventInvokedResponse();
  }
  #endregion
}
