using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Roles.Models;
using Pocco.Svc.Roles.Repositories;
using System.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security;

namespace RoleService.Services;

public class OrganizationRoleService : V0RoleService.V0RoleServiceBase {
    private readonly IRoleRepository _repo;
    public OrganizationRoleService(IRoleRepository repo) {
        _repo = repo;
    }

    public override async Task<V0GetReply> Get(V0GetRequest request, ServerCallContext callContext) {
        var role = GetByIdAsync(request.Id);
        if (role == null) {
            throw new RpcException(new Status(StatusCode.NotFound, "Role not found"));
        }
        return ToV0GetReply(role);
    }

    public override async Task<V0CreateReply> Create(V0CreateRequest request, ServerCallContext callContext) {
        var model = new Role {
            Name = request.Name,
            Description = Console.ReadLine()?.Trim() ?? string.Empty,
            Permissions = (Console.ReadLine()?.Trim() ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList(),
            Created_At = DateTime.UtcNow,
            Updated_At = DateTime.UtcNow
        };
        var created = await _repo.CreateAsync(request.OrgId, model);
        return new V0CreateReply { Rolemodel = MapToGrpc(created) };
    }

    public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
        var updates = new List<UpdateDefinition<Role>>();
        var updateDataBuilder = Builders<Role>.Update;

        if (!string.IsNullOrWhiteSpace(request.Rolemodel.Name))
            updates.Add(updateDataBuilder.Set(r => r.Name, request.Rolemodel.Name));
        if (!string.IsNullOrWhiteSpace(request.Rolemodel.Descriptions))
            updates.Add(updateDataBuilder.Set(r => r.Description, request.Rolemodel.Descriptions));
        if (request.Rolemodel.Permissions != null && request.Rolemodel.Permissions.Count > 0)
            updates.Add(updateDataBuilder.Set(r => r.Permissions, request.Rolemodel.Permissions.ToList()));
        if (updates.Count == 0) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "No fields to update"));
        }
        var updateDefinition = updateDataBuilder.Combine(updates);
        var collection = GetCollection(request.OrgId);
        var updated = await _repo.UpdateAsync(request.OrgId, updateDefinition);
        /*updates.Add(updateDataBuilder
        .Set(ro => ro.Name, request.Name)
        .Set(ro => ro.Description, request.Descriptions)
        .Set(ro => ro.Permissions, request.Permissions.Split(',').Select(s => s.Trim()).ToList()
        ));
        var updateDefinition = updateDataBuilder.Combine(updates);
        await _repo.UpdateAsync(request.OrgId, updateDefinition);
        return new Empty();*/
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

