using Grpc.Core;
using CoreAPI_Service = Pocco.Libs.Protobufs.CoreAPI.Services;
using OrganizationsInfo_Service = Pocco.Libs.Protobufs.Organizations_Info.Services;
using OrganizationsMember_Service = Pocco.Libs.Protobufs.Organizations_Member.Services;
using OrganizationsChat_Service = Pocco.Libs.Protobufs.Organizations_Chat.Services;
using OrganizationsRole_Service = Pocco.Libs.Protobufs.Organizations_Role.Services;
using Pocco.Libs.Protobufs.Organizations_Info.Types;
using Pocco.Libs.Protobufs.Organizations_Member.Types;
using Pocco.Libs.Protobufs.Organizations_Message.Services;
using Pocco.Libs.Protobufs.Organizations_Role.Types;
using Pocco.Libs.Protobufs.Organizations_Chat.Types;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl {
  private readonly OrganizationsInfo_Service.V0OrganizationInfoService.V0OrganizationInfoServiceClient _orgInfoService;
  private readonly OrganizationsMember_Service.V0OrganizationMemberService.V0OrganizationMemberServiceClient _orgMemberService;
  private readonly OrganizationsChat_Service.V0OrganizationChatService.V0OrganizationChatServiceClient _orgChatService;
  private readonly OrganizationMessageRpcService.OrganizationMessageRpcServiceClient _orgMessageService;
  private readonly OrganizationsRole_Service.V0RoleService.V0RoleServiceClient _orgRoleService;

  #region Informations
  public override async Task<CoreAPI_Service.Organization> Get(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
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
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgInfoService.CreateAsync(new V0CreateOrganizationRequest { // TODO: 名前空間の整理
      Name = request.Base.Name,
      Description = request.Description,
      CreatedBy = getAccountId,
      InvokedBy = getAccountId
    });
    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateOrganization(CoreAPI_Service.V0UpdateOrganizationRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgInfoService.UpdateAsync(new V0UpdateOrganizationRequest {
      Id = request.OrganizationId,
      Name = request.Name,
      Description = request.Description,
      InvokedBy = getAccountId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> Delete(CoreAPI_Service.V0BaseRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgInfoService.DeleteAsync(new V0DeleteOrganizationRequest {
      Id = request.Id,
      InvokedBy = getAccountId
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
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgMemberService.JoinAsync(new V0JoinRequest {
      UserId = request.UserId,
      OrganizationId = request.OrganizationId,
      Code = request.InviteCode,
      InvokedBy = getAccountId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> LeaveMember(CoreAPI_Service.V0LeaveMemberRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgMemberService.LeaveAsync(new V0LeaveRequest {
      MemberId = request.UserId,
      OrganizationId = request.OrganizationId,
      InvokedBy = getAccountId
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
        OrganizationId = role.OrgId,
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
      OrganizationId = reply.Rolemodel.OrgId,
      Name = reply.Rolemodel.Name,
      CreatedAt = reply.Rolemodel.CreatedAt,
      UpdatedAt = reply.Rolemodel.UpdatedAt
    };
    role.Permissions.AddRange(reply.Rolemodel.Permissions);

    return role;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> CreateRole(CoreAPI_Service.V0CreateRollRequest request, ServerCallContext context) {
    var reply = await _orgRoleService.CreateAsync(new Libs.Protobufs.Organizations_Role.Types.V0CreateRequest {
      Name = request.Base.Name,
      OrgId = request.OrganizationId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateRole(CoreAPI_Service.Role request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var updateRequest = new Libs.Protobufs.Organizations_Role.Types.V0UpdateRequest {
      Rolemodel = new V0RoleModel {
        Id = request.RoleId,
        OrgId = request.OrganizationId,
        Name = request.Name,
        Permissions = { request.Permissions },
        CreatedAt = request.CreatedAt,
        UpdatedAt = request.UpdatedAt,
      },
      InvokedBy = getAccountId
    };

    var reply = await _orgRoleService.UpdateAsync(updateRequest);

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> DeleteRole(CoreAPI_Service.V0DeleteXRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgRoleService.DeleteAsync(new Libs.Protobufs.Organizations_Role.Types.V0DeleteRequest {
      Id = request.Id,
      OrgId = request.OrganizationId,
      InvokedBy = getAccountId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }
  #endregion

  #region Chats
  public override async Task<CoreAPI_Service.V0ListChatsResponse> ListChats(CoreAPI_Service.V0ListXRequest request, ServerCallContext context) {
    var reply = await _orgChatService.GetListAsync(new Libs.Protobufs.Organizations_Chat.Types.V0GetListRequest {
      OrgId = request.Base.Id
    });

    var response = new CoreAPI_Service.V0ListChatsResponse();

    foreach (var chat in reply.Chats) {
      var c = new CoreAPI_Service.Chat {
        ChatId = chat.Id,
        OrganizationId = chat.OrgId,
        Name = chat.Name,
        Description = chat.Description,
        CreatedAt = chat.CreatedAt,
        UpdatedAt = chat.UpdatedAt
      };

      response.Chats.Add(c);
    }

    return response;
  }

  public override async Task<CoreAPI_Service.Chat> GetChat(CoreAPI_Service.V0GetXRequest request, ServerCallContext context) {
    var reply = await _orgChatService.GetAsync(new Libs.Protobufs.Organizations_Chat.Types.V0GetRequest {
      Id = request.Id,
      OrgId = request.OrganizationId
    });

    var chat = new CoreAPI_Service.Chat {
      ChatId = reply.Chatsmodel.Id,
      OrganizationId = reply.Chatsmodel.OrgId,
      Name = reply.Chatsmodel.Name,
      Description = reply.Chatsmodel.Description,
      CreatedAt = reply.Chatsmodel.CreatedAt,
      UpdatedAt = reply.Chatsmodel.UpdatedAt
    };

    return chat;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> CreateChat(CoreAPI_Service.V0CreateChatRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgChatService.CreateAsync(new Libs.Protobufs.Organizations_Chat.Types.V0CreateRequest {
      OrgId = request.OrganizationId,
      Name = request.Base.Name,
      Type = request.Type,
      CreatedBy = getAccountId,
      InvokedBy = getAccountId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }
  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateChat(CoreAPI_Service.V0UpdateChatRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var updateRequest = new Libs.Protobufs.Organizations_Chat.Types.V0UpdateRequest {
      Chatsmodel = new V0ChatsModel {
        Id = request.ChatId,
        OrgId = request.OrganizationId,
        Name = request.Name,
        Description = request.Description,
        CreatedBy = getAccountId
      },
      InvokedBy = getAccountId
    };
    var reply = await _orgChatService.UpdateAsync(updateRequest);

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> DeleteChat(CoreAPI_Service.V0DeleteXRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgChatService.DeleteAsync(new Libs.Protobufs.Organizations_Chat.Types.V0DeleteRequest {
      Id = request.Id,
      OrgId = request.OrganizationId,
      InvokedBy = getAccountId
    });

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.EventId
    };
  }
  #endregion

  #region Messages
  public override async Task<CoreAPI_Service.V0ListMessagesResponse> ListMessages(CoreAPI_Service.V0ListMessagesRequest request, ServerCallContext context) {
    var reply = await _orgMessageService.GetMessageListInOrganizationAsync(new Libs.Protobufs.Organizations_Message.Types.V0GetMessageListInOrganizationRequest {
      ChatId = request.ChatId,
    });

    var response = new CoreAPI_Service.V0ListMessagesResponse();

    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    foreach (var message in reply.Messages) {
      var m = new CoreAPI_Service.Message {
        MessageId = message.Id,
        OrganizationId = message.OrganizationId,
        ChatId = message.ChatId,
        SenderId = getAccountId,
        Content = message.Content,
        CreatedAt = message.CreatedAt,
        UpdatedAt = message.UpdatedAt,
      };

      response.Messages.Add(m);
    }

    return response;
  }

  public override async Task<CoreAPI_Service.Message> GetMessage(CoreAPI_Service.V0GetMesssageRequest request, ServerCallContext context) {
    var reply = await _orgMessageService.GetMessageInOrganizationAsync(new Libs.Protobufs.Organizations_Message.Types.V0GetMessageInOrganizationRequest {
      OrganizationId = request.OrganizationId,
      ChatId = request.ChatId,
      MessageId = request.MessageId
    });

    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var message = new CoreAPI_Service.Message {
      MessageId = reply.Message.Id,
      OrganizationId = reply.Message.OrganizationId,
      ChatId = reply.Message.ChatId,
      SenderId = getAccountId,
      Content = reply.Message.Content,
      CreatedAt = reply.Message.CreatedAt,
      UpdatedAt = reply.Message.UpdatedAt,
    };

    return message;
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> CreateMessage(CoreAPI_Service.Message request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgMessageService.TrySendMessageToOrganizationAsync(new Libs.Protobufs.Organizations_Message.Types.V0TrySendMessageToOrganizationRequest {
      ChatId = request.ChatId,
      OrganizationId = request.OrganizationId,
      SenderId = getAccountId,
      Content = request.Content,
    });
    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.MessageSentEventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> UpdateMessage(CoreAPI_Service.Message request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var updateRequest = new Libs.Protobufs.Organizations_Message.Types.V0TryUpdateMessageInOrganizationRequest {
      MessageId = request.MessageId,
      OrganizationId = request.OrganizationId,
      ChatId = request.ChatId,
      SenderId = getAccountId,
      Content = request.Content,
      CreatedAt = request.CreatedAt,
      UpdatedAt = request.UpdatedAt,
      
    };
    var reply = await _orgMessageService.TryUpdateMessageInOrganizationAsync(updateRequest);

    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.MessageSentEventId
    };
  }

  public override async Task<CoreAPI_Service.V0EventInvokedResponse> DeleteMessage(CoreAPI_Service.V0DeleteMessageRequest request, ServerCallContext context) {
    string getAccountId = context.RequestHeaders.GetValue("x-account-id") ?? "unkown";

    var reply = await _orgMessageService.TryDeleteMessageFromOrganizationAsync(new Libs.Protobufs.Organizations_Message.Types.V0TryDeleteMessageFromOrganizationRequest {
      MessageId = request.MessageId,
      ChatId = request.ChatId,
      OrganizationId = request.OrganizationId,
      SenderId = getAccountId
    });
    return new CoreAPI_Service.V0EventInvokedResponse {
      EventId = reply.MessageSentEventId
    };
  }
  #endregion
}
