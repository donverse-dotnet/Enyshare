using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Roles.Models;


namespace RoleService.Services;

public class OrganizationRoleService : V0RoleService.V0RoleServiceBase {
  private readonly IRoleRepository _repo;
  public OrganizationRoleService(IRoleRepository repo) {
    _repo = repo;
  }

  public override async Task<V0GetReply> Get(V0GetRequest request, ServerCallContext callContext) {
    var role = await _repo.GetByIdAsync(request.Id, request.Id);
    if (role == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
    }
    return new V0GetReply { Rolemodel = MapToGrpc(role) };
  }

  public override async Task<V0CreateReply> Create(V0CreateRequest request, ServerCallContext callContext) {
    var model = new Role {
      Name = request.Name,
      Description = ,
      Permissions = ,
      Created_At = DateTime.UtcNow,
      Updated_At = DateTime.UtcNow
    };
    var created = await _repo.CreateAsync(request.OrgId, model);
    return new V0CreateReply { Rolemodel = MapToGrpc(created) };
  }

  public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
    var updateRole = new Role {
      Name = request.Rolemodel.Name,
      Description = request.Rolemodel.Descriptions,
      Permissions = request.Rolemodel.Permissions.ToList(),
      Updated_At = DateTime.UtcNow
    };

    var updated = await _repo.UpdateAsync(request.OrgId, updateRole);
    if (updated == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Role not found or no fields to update"));
    }

    return new Empty();
  }

  public override async Task<V0DeleteReply> Delete(V0DeleteRequest request, ServerCallContext context) {
    var success = await _repo.DeleteAsync(request.OrgId, request.Id);
    if (!success) {
      return new V0DeleteReply {
        StatusCode = 404,
        Message = "Role not found",
        Success = false
      };
    }
    return new V0DeleteReply {
      StatusCode = 200,
      Message = "Role deleted successfully",
      Success = true
    };
  }
  private V0RoleModel MapToGrpc(Role role) => new V0RoleModel {
    Id = role.Id,
    Name = role.Name,
    Descriptions = role.Description,
    Permissions = { role.Description },
    CreatedAt = Timestamp.FromDateTime(role.Created_At.ToUniversalTime()),
    UpdatedAt = Timestamp.FromDateTime(role.Updated_At.ToUniversalTime())
  };
}

