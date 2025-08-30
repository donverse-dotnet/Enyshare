using System.Security;

using Grpc.Core;

using Microsoft.AspNetCore.Authorization.Infrastructure;

using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Roles.Models;

namespace RoleService.Services;

public class OrganizationRoleService : V0OrganizationRoleService.V0OrganizationRoleServiceBase {
    private readonly IMongoCollection<Role> _roles;
    public OrganizationRoleService(IMongoClient mongoClient) {
        var database = mongoClient.GetDatabase("Entities");
        _roles = database.GetCollection<Role>("Organizations");
    }

    /*public override async Task<V0CreateReply> Create(V0CreateRequest request, ServerCallContext callContext) {
        var model = new Role {
            Name = request.Name,
            Description =
        };
        
    }*/
}

