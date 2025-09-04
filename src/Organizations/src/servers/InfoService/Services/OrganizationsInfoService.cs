using Grpc.Core;

using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NameCheckService;
using Pocco.Libs.Protobufs.Services;
using Google.Protobufs.WellKnownTypes;

using System.ComponentModel;
using System.Threading.Tasks;

namespace InfoService.Services;

// gRPCサービスの実装クラス：組織情報に関する操作を提供
public class OrganizationsInfoServiceImpl : V0OrganizationInfoService.V0OrganizationInfoServiceBase
{
  private readonly IMongoDatabase _mongo;
  private readonly IMongoCollection<OrganizationEntity> _orgs;
  private readonly IMongoCollection<MemberEntity> _members;
  private readonly IMongoCollection<RoleEntity> _roles;
  private readonly IMongoCollection<ChatEntity> _chats;

  // コンストラクタ：MongoDBコレクションとバリデータを注入
  public OrganizationsInfoServiceImpl(IMongoDatabase mongo)
  {
    _mongo = mongo;
    _orgs = mongo.GetCollection<OrganizationEntity>("organizations");
    _members = mongo.GetCollection<MemberEntity>("members");
    _roles = mongo.GetCollection<RoleEntity>("roles");
    _chats = mongo.GetCollection<ChatEntity>("chats");
  }

  // Create: 組織を新規作成し、関連データも初期化
  public override async Task<CreateOrganizationReply> Create(CreateOrganizationRequest request, ServerCallContext context)
  {
    // 組織名の重複チェック　(論理削除されていないもの)
    var exists = await _orgs.Find(x => x.Name == request.Name && x.DeletedAt == null).AnyAsync();
    if (exists)
    {
      throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization name already exists"));
    }

    // 組織データの作成
    var org = new OrganizationEntity
    {
      Id = ObjectId.GenerateNewId().ToString(),
      Name = request.Name,
      Description = request.Description,
      CreatedBy = request.CreatedBy,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
      DeletedAt = null
    };

    // MongoDBに保存
    await _orgs.InsertOneAsync(org);

    // 作成者をメンバーとして登録
    var member = new MemberEntity
    {
      Id = ObjectId.GenerateNewId().ToString(),
      OrganizationId = org.Id,
      UserId = request.CreatedBy,
      Role = "owner",
      JoinedAt = DataTime.UtcNow
    };
    await _members.InsertOneAsync(member);

    // デフォルトロールを作成
    var role = new RoleEntity
    {
      Id = ObjectId.GenerateNewId().ToString(),
      OrganizationId = org.Id,
      Name = "default",
      Permissions = new[] { "read", "write" }
    };
    await _roles.InsertOneAsync(role);

    // デフォルトチャットを作成
    var chat = new ChatEntity
    {
      Id = ObjectId.GenerateNewId().ToString(),
      OrganizationId = org.Id,
      Name = "general",
      CreatedAt = DataTime.UtcNow
    };
    await _chats.InsertOneAsync(chat);

    // 作成された組織情報を返却
    return new CreateOrganizationReply
    {
      Organization = new V0InfoModel
      {
        Id = org.Id,
        Name = org.Name,
        Description = org.Description,
        CreatedBy = org.CreateBy,
        CreatedAt = Timestamp.FromDataTime(org.CreatedAt.ToUniversalTime()),
        UpdatedAt = Timestamp.FromDataTime(org.UpdatedAt.ToUniversalTime()),
        DeletedAt = null
      }
    };
  }

  // Update: 組織情報を更新（重複チェックあり）
  public override async Task<DeleteOrganizationReply> Update(DeleteOrganizationRequest request, ServerCallContext context)
  {
    var conflict = await _orgs.Find(x => x.Name == request.Name && x.Id != request.Id && x.DeleteAt == null).AnyAsync();
    if (conflict)
    {
      throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization name already exists"));
    }

    // 更新処理
    var update = Builders<OrganizationEntity>.Update
    .Set(x => x.Name, request.Name)
    .Set(x => x.Description, request.Description)
    .Set(x => x.UpdatedAt, DateTime.UtcNow);

    var result = await _orgs.UpdateOneAsync(x => x.Id == request.Id && x.DeletedAt == null, update);
    if (result.MatchedCount == 0)
    {
      throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
    }

    // 更新後のデータ取得
    var updated = await _orgs.Find(x => x.Id == request.Id).FirstOrDefaultAsync();

    return new CreateOrganizationReply
    {
      Organization = new V0InfoModel
      {
        Id = updated.Id,
        Name = updated.Name,
        Description = updated.Description,
        CreatedBy = updated.CreateBy,
        CreatedAt = Timestamp.FromDataTime(updated.CreatedAt.ToUniversalTime()),
        UpdatedAt = TimeStamp.FromDataTime(updated.UpdatedAt.ToUniversalTime()),
        DeletedAt = updated.DeletedAt.HasValue
            ? Timestamp.FromDataTime(updated.DeletedAt.Value.ToUniversalTime())
            : null
      }
    };
  }

  // Delete: 組織データベースを削除（関連データも含む）
  public override async Task<DeleteOrganizationReply> Delete(DeleteOrganizationRequest request, ServerCallContext context)
  {
    // 一時的に全データをキャッシュ（必要ならここで取得）
    var org = await _orgs.Find(x => x.Id == request.Id).FirstOrDefaultAsync();
    if (org == null)
    {
      throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
    }

    // 組織データの論理削除
    var update = Builders<OrganizationEntity>.Update.Set(X => X.DeletedAt, DateTime.UtcNow);
    await _orgs.UpdateOneAsync(x => x.Id == request.Id, update);

    // 関連データの物理削除（必要に応じて論理削除に変更可能）
    await _members.DeleteManyAsync(x => x.OrganizationId == request.Id);
    await _roles.DeleteManyAsync(x => x.OrganizationId == request.Id);
    await _chats.DeleteManyAsync(x => x.OrganizationId == request.Id);

    return new DeleteOrganizationReply
    {
      Success = true,
      Message = "Organization and related data deleted successfully."
    };
  }

  // Get: 組織情報を取得
  public override async Task<V0InfoModel> GetInfo(GetOrganizationInfoRequest request, ServerCallContext context)
  {
    var org = await _orgs.Find(x => x.Id == request.Id && x.DeletedAt == null).FirstOrDefaultAsync();
    if (org == null)
      throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));

    return new V0InfoModel
    {
      Id = org.Id,
      Name = org.Name,
      Description = org.Description,
      CreatedBy = org.CreatedBy,
      CreatedAt = Timestamp.FromDateTime(org.CreatedAt.ToUniversalTime()),
      UpdatedAt = Timestamp.FromDateTime(org.UpdatedAt.ToUniversalTime()),
      DeletedAt = null
    };
  }
}
