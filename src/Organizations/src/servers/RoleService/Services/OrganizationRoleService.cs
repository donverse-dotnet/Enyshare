using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;
using Pocco.Svc.Roles.Models;

namespace RoleService.Services;

public class OrganizationRoleService : V0RoleService.V0RoleServiceBase {
    private readonly IMongoCollection<Role> _roles;
    public OrganizationRoleService(IMongoClient mongoClient) {
        var database = mongoClient.GetDatabase("Entities");
        _roles = database.GetCollection<Role>("Organizations");
    }

    public override async Task<V0CreateReply> Create(V0CreateRequest request, ServerCallContext callContext) {
#pragma warning disable CS8601 // Null 参照代入の可能性があります。
        var model = new Role {
            Name = request.Name,
            Description = Console.ReadLine()?.Trim(),
            Permissions = Console.ReadLine()?.Trim()?
                .Split(',')                             //カンマで分割
                .Select(p => p.Trim())                  //各要素の前後の空白を削除
                .Where(p => !string.IsNullOrEmpty(p))   //空要素を除外
                .ToList() ?? new List<string>(),         //null対策
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _roles.InsertOneAsync(model);
        return new V0CreateReply {

        };
    }

    /*public override async Task<Empty> Update(V0UpdateRequest request, ServerCallContext context) {
        var filter = Builders<Role>.Filter.Eq(a => a.Id, ObjectId.Parse(request.Id));
        var updateDataBuilder = Builders<Role>.Update;
        var updates = new List<UpdateDefinition<Role>>();

        updates.Add(updateDataBuilder
        .Set(ro => ro.Name, request.Name)
        .Set(ro => ro.Description, request.Descriptions)
        .Set(ro => ro.Permissions, request.Permissions.Split(',').Select(s => s.Trim()).ToList())
        );

        await _roles.UpdateOneAsync(filter, updateDataBuilder.Combine(updates));
        return new Empty();
    }*/
}

