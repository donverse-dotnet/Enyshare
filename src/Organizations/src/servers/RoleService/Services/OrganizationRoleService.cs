using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Roles.Models;


namespace RoleService.Services;

public class OrganizationRoleService : V0RoleService.V0RoleServiceBase {
  private readonly IRoleRepository _repo;
  private readonly ILogger<OrganizationRoleService> _logger;
  public OrganizationRoleService([FromServices] IRoleRepository repo, [FromServices] ILogger<OrganizationRoleService> logger) {
    _repo = repo;
    _logger = logger;

    _logger.LogInformation("OrganiationRoleService is initialized!");
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

    var model = new Role() {
      Id = ObjectId.GenerateNewId().ToString(),
      OrgId = request.OrgId,
      Name = request.Name,
      Description = "", // 作成するときは空のまま
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };
    Role createdRole = await _repo.CreateAsync(request.OrgId, model);
    _logger.LogInformation("{RoleId} is successfully created on {OrgId}", createdRole.Id, request.OrgId);
    return new V0RoleChangesReply {
      EventId = "fake id" //TODO eventbridgeからのidに置き換える
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

    return new V0RoleChangesReply {
      EventId = "fake id" //TODO eventbridgeからのidに置き換える
    };
  }

  public override async Task<V0RoleChangesReply> Delete(V0DeleteRequest request, ServerCallContext context) {
    var success = await _repo.DeleteAsync(request.OrgId, request.Id);
    if (!success) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found or no fields to delete"));
    }

    return new V0RoleChangesReply {
      EventId = "fake id" //TODO eventbridgeからのidに置き換える
    };
  }
}
