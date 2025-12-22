using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;

using Pocco.Libs.Protobufs.EventBridge.Enums;
using Pocco.Libs.Protobufs.EventBridge.Services;
using Pocco.Libs.Protobufs.EventBridge.Types;
using Pocco.Libs.Protobufs.Organizations_Role.Services;
using Pocco.Libs.Protobufs.Organizations_Role.Types;
using Pocco.Svc.Roles.Models;

namespace RoleService.Services;

public class V0InternalRoleServiceImpl : V0InternalOrganizationRoleService.V0InternalOrganizationRoleServiceBase {
  private readonly IRoleRepository _repo;
  private readonly ILogger<OrganizationRoleService> _logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;
  public V0InternalRoleServiceImpl(
    [FromServices] IRoleRepository repo,
    [FromServices] ILogger<OrganizationRoleService> logger,
    [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge
  ) {
    _repo = repo;
    _logger = logger;
    _eventBridge = eventBridge;

    _logger.LogInformation("OrganiationRoleService is initialized!");
  }
  public override async Task<V0RoleModel> Create(V0CreateRequest request, ServerCallContext context) {

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
      EventType = "OnOrganizationRoleCreated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    var permString = string.Join(",", createdRole.Permissions);

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrgId}" });
    newEventData.Payload.Fields.Add("role_id", new Value { StringValue = $"{createdRole.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{createdRole.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{createdRole.Description}" });
    newEventData.Payload.Fields.Add("permissions", new Value { StringValue = $"{permString}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{createdRole.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{createdRole.UpdatedAt}" });

    await _eventBridge.NewEventAsync(newEventData);

    return createdRole.ToV0RoleModel();
  }
}
