using Google.Protobuf.WellKnownTypes;

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

  public override async Task<Empty> Get(V0GetRequest request, ServerCallContext context) {
    var role = await _repo.GetByIdAsync(request.OrgId, request.Id);
    if (role == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
    }
    return new Empty();
  }

  public override async Task<Empty> Create(V0CreateRequest request, ServerCallContext context) {

    var model = new Role() {
      Id = ObjectId.GenerateNewId().ToString(),
      Name = request.Name,
      // Org_Id = request.OrgId,
      Description = "", // 作成するときは空のまま
      // Permissions    // 作成するときは空のまま
      Created_At = DateTime.UtcNow,
      Updated_At = DateTime.UtcNow
    };
    Role createdRole = await _repo.CreateAsync(request.OrgId, model);
    _logger.LogInformation("{RoleId} is successfully created on {OrgId}", createdRole.Id, request.OrgId);
    return new Empty();
  }

  public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
    var updateRole = new Role {
      Id = request.Rolemodel.Id,
      Name = request.Rolemodel.Name,
      Description = request.Rolemodel.Descriptions,
      Permissions = request.Rolemodel.Permissions.ToList(),
      Updated_At = DateTime.UtcNow
    };

    var updated = await _repo.TryUpdateAsync(request.OrgId, request.Rolemodel.Id, updateRole);
    if (updated == false) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found or no fields to update"));
    }

    return new Empty();
  }

  public override async Task<Empty> Delete(V0DeleteRequest request, ServerCallContext context) {
    var success = await _repo.DeleteAsync(request.OrgId, request.Id);
    if (!success) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found or no fields to delete"));
    }

    return new Empty();
  }
}
