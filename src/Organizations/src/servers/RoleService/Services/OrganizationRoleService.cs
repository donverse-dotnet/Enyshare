using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.EventBridge.Protobufs.Enums;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;
using Pocco.Svc.Roles.Models;
using Pocco.Svc.Roles.Repositories;



namespace RoleService.Services;

public class OrganizationRoleService : V0RoleService.V0RoleServiceBase {
  private readonly IRoleRepository _repo;
  private readonly ILogger<OrganizationRoleService> _logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;
  public OrganizationRoleService([FromServices] IRoleRepository repo, [FromServices] ILogger<OrganizationRoleService> logger, [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge) {
    _repo = repo;
    _logger = logger;
    _eventBridge = eventBridge;

    _logger.LogInformation("OrganiationRoleService is initialized!");
  }

  public override async Task<V0GetListReply> GetList(V0GetListRequest request, ServerCallContext context) {
    var roles = await _repo.GetListAsync(request.OrgId);
    var reply = new V0GetListReply();
    reply.Roles.AddRange(roles.Select(r => {
      var model = new V0RoleModel {
        Id = r.Id,
        OrgId = r.OrgId,
        Name = r.Name,
        Descriptions = r.Description,
        CreatedAt = Timestamp.FromDateTime(r.CreatedAt),
        UpdatedAt = Timestamp.FromDateTime(r.UpdatedAt)
      };
      model.Permissions.AddRange(r.Permissions);

      return model;
    }));

    return reply;
  }
  public override async Task<V0GetReply> Get(V0GetRequest request, ServerCallContext context) {
    var role = await _repo.GetByIdAsync(request.OrgId, request.Id);
    if (role == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
    }
    return new V0GetReply {
      Rolemodel = role.ToV0RoleModel()
    };
  }

  public override async Task<V0RoleChangesReply> Create(V0CreateRequest request, ServerCallContext context) {

    var currentTime = DateTime.UtcNow;

    var model = new Role() {
      Id = ObjectId.GenerateNewId().ToString(),
      OrgId = request.OrgId,
      Name = request.Name,
      Description = "", // 作成するときは空のまま
      CreatedAt = currentTime,
      UpdatedAt = currentTime
    };
    Role createdRole = await _repo.CreateAsync(request.OrgId, model);
    _logger.LogInformation("{RoleId} is successfully created on {OrgId}", createdRole.Id, request.OrgId);

    // イベントを伝搬させるのをEventBridgeに依頼
    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnRoleCreated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("ornigazation_id", new Value { StringValue = $"{request.OrgId}" });
    newEventData.Payload.Fields.Add("role_id", new Value { StringValue = $"{createdRole.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{createdRole.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{createdRole.Description}" });
    newEventData.Payload.Fields.Add("permissions", new Value { StringValue = $"{createdRole.Permissions}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{createdRole.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{createdRole.UpdatedAt}" });

    var createdEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0RoleChangesReply {
      EventId = createdEventData.EventId
    };
  }

  public override async Task<V0RoleChangesReply> Update(V0UpdateRequest request, ServerCallContext context) {
    var updateRole = new Role {
      Id = request.Rolemodel.Id,
      OrgId = request.Rolemodel.OrgId,
      Name = request.Rolemodel.Name,
      Description = request.Rolemodel.Descriptions,
      Permissions = request.Rolemodel.Permissions.ToList(),
      UpdatedAt = DateTime.UtcNow
    };

    var updated = await _repo.TryUpdateAsync(request.Rolemodel.OrgId, request.Rolemodel.Id, updateRole);
    if (updated == false) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found or no fields to update"));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnRoleUpdated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("ornigazation_id", new Value { StringValue = $"{request.Rolemodel.OrgId}" });
    newEventData.Payload.Fields.Add("role_id", new Value { StringValue = $"{updateRole.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{updateRole.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{updateRole.Description}" });
    newEventData.Payload.Fields.Add("permissions", new Value { StringValue = $"{updateRole.Permissions}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{updateRole.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{updateRole.UpdatedAt}" });

    var updatedEventData = _eventBridge.NewEvent(
        newEventData
    );

    return new V0RoleChangesReply {
      EventId = updatedEventData.EventId
    };
  }

  public override async Task<V0RoleChangesReply> Delete(V0DeleteRequest request, ServerCallContext context) {
    var success = await _repo.DeleteAsync(request.OrgId, request.Id);
    if (!success) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found or no fields to delete"));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnRoleDeleted",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("ornigazation_id", new Value { StringValue = $"{request.OrgId}" });
    newEventData.Payload.Fields.Add("role_id", new Value { StringValue = $"{request.Id}" });

    var deletedEventData = _eventBridge.NewEvent(
        newEventData
    );

    return new V0RoleChangesReply {
      EventId = deletedEventData.EventId
    };
  }
}
