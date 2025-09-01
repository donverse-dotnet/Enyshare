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

namespace RoleService.Services;

public class OrganizationRoleService : V0RoleService.V0RoleServiceBase {
    private readonly IMongoClient _mongoClient;
    public OrganizationRoleService(IMongoClient mongoClient) {
        _mongoClient = mongoClient;
    }
    private RoleRepository GetRepository(string org_Id) => new RoleRepository(_mongoClient, org_Id);
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
        var repo = GetRepository(request.OrgId);
        await repo.CreateAsync(model);

        return new V0CreateReply {

        };
    }

    public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
        var repo = GetRepository(request.OrgId);
        var role = await repo.GetByIdAsync(request.OrgId, request.Id)
        updates.Add(updateDataBuilder
        .Set(ro => ro.Name, request.Name)
        .Set(ro => ro.Description, request.Descriptions)
        .Set(ro => ro.Permissions, request.Permissions.Split(',').Select(s => s.Trim()).ToList())
        );

        await _roles.UpdateOneAsync(filter, updateDataBuilder.Combine(updates));
        return new Empty();
    }
}

