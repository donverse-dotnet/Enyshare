using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using MongoDB.Bson;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Accounts.Enums;
using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.Accounts.Types;
using Pocco.Svc.Accounts.Models;

namespace Pocco.Svc.Accounts.Services;

public class InternalAccountServiceImpl : V0InternalAccountService.V0InternalAccountServiceBase {
  private readonly IMongoCollection<Account> _accounts;

  public InternalAccountServiceImpl(IMongoClient mongoClient) {
    var database = mongoClient.GetDatabase("Entities");
    _accounts = database.GetCollection<Account>("Accounts");
  }

  public override async Task<Empty> UpdateOrgList(V0UpdateOrgListRequest request, ServerCallContext context) {
    var target = await _accounts.FindAsync(acc => acc.Id == ObjectId.Parse(request.AccountId)).Result.FirstOrDefaultAsync()
                 ?? throw new RpcException(new Status(StatusCode.NotFound, "Account not found"));

    var update = request.Action switch {
      V0OrgListUpdateActions.Add => Builders<Account>.Update.AddToSet(acc => acc.OrganizationIds, request.OrgId),
      V0OrgListUpdateActions.Remove => Builders<Account>.Update.Pull(acc => acc.OrganizationIds, request.OrgId),
      _ => throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid action"))
    };

    var result = await _accounts.UpdateOneAsync(acc => acc.Id == target.Id, update);

    if (result.MatchedCount == 0) {
      throw new RpcException(new Status(StatusCode.NotFound, "Account not found during update"));
    }

    return new Empty();
  }
}
