// gRPCの基本クラスとステータスコードを使用するための名前空間
using Grpc.Core;

// gRPCのTimestamp型を使用するための名前空間（スペルミス修正済み）
using Google.Protobuf.WellKnownTypes;

// MongoDBの操作に必要な名前空間
using MongoDB.Bson;
using MongoDB.Driver;

// サービス定義とプロトコルバッファの型定義
using Pocco.Libs.Protobufs.Organizations_Info.Services;
using Pocco.Libs.Protobufs.Organizations_Info.Types;
using InfoService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.EventBridge.Services;
using Pocco.Libs.Protobufs.EventBridge.Types;
using Pocco.Libs.Protobufs.EventBridge.Enums;
using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.Accounts.Types;
using Pocco.Libs.Protobufs.Accounts.Enums;
using Pocco.Libs.Protobufs.Organizations_Chat.Services;
using Pocco.Libs.Protobufs.Organizations_Chat.Types;

namespace InfoService.Services;

// gRPCサービスの実装クラス：組織情報のCRUD操作を提供
public class OrganizationsInfoServiceImpl : V0OrganizationInfoService.V0OrganizationInfoServiceBase {
  // MongoDBの各コレクションを保持（DIで注入）
  private readonly IMongoCollection<OrganizationEntity> _orgs;
  private readonly V0InternalAccountService.V0InternalAccountServiceClient _internalAccountSvc;
  private readonly V0InternalOrganizationChatService.V0InternalOrganizationChatServiceClient _internalOrgChatSvc;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;
  private readonly ILogger<OrganizationsInfoServiceImpl> _logger;

  // コンストラクタ：MongoDBインスタンスから必要なコレクションを取得
  public OrganizationsInfoServiceImpl(
    [FromServices] IMongoDatabase mongo,
    [FromServices] V0InternalAccountService.V0InternalAccountServiceClient internalAccountSvc,
    [FromServices] V0InternalOrganizationChatService.V0InternalOrganizationChatServiceClient internalOrgChatSvc,
    [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge,
    [FromServices] ILogger<OrganizationsInfoServiceImpl> logger
  ) {
    _orgs = mongo.GetCollection<OrganizationEntity>("Organizations");
    _internalAccountSvc = internalAccountSvc;
    _internalOrgChatSvc = internalOrgChatSvc;
    _eventBridge = eventBridge;
    _logger = logger;

    _logger.LogInformation("OrganizationsInfoServiceImpl is initialized!");
  }

  // 組織の新規作成処理
  // - 組織名の重複チェック（論理削除されていないもののみ対象）
  // - 組織エンティティの作成と保存
  // - 作成者を初期メンバーとして登録
  // - デフォルトロールとチャットの初期化
  public override async Task<V0InfoChangesReply> Create(V0CreateOrganizationRequest request, ServerCallContext context) {
    // 組織名の重複チェック（DeletedAtがnullのもののみ対象）
    var exists = await _orgs.Find(x => x.Name == request.Name && x.DeletedAt == null).AnyAsync();
    if (exists) {
      // 重複がある場合は gRPC の AlreadyExists ステータスを返す
      throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization name already exists"));
    }

    var currentTime = DateTime.UtcNow;

    // 組織エンティティの作成
    var org = new OrganizationEntity {
      Id = ObjectId.GenerateNewId().ToString(),
      Name = request.Name,
      Description = request.Description,
      CreatedBy = request.CreatedBy,
      CreatedAt = currentTime,
      UpdatedAt = currentTime,
      DeletedAt = null
    };

    // MongoDBに保存
    await _orgs.InsertOneAsync(org);
    var createdOrg = _orgs.FindAsync(item => item.Name == request.Name).Result.ToListAsync().Result.FirstOrDefault() ?? throw new RpcException(new Status(StatusCode.NotFound, "Organization maybe created but can't found."));
    var createdChat = await _internalOrgChatSvc.CreateAsync(new V0CreateRequest {
      OrgId = createdOrg.Id,
      Name = "General",
      Type = "text",
      CreatedBy = request.CreatedBy,
      InvokedBy = request.InvokedBy
    });

    // アカウントを更新
    var reply = await _internalAccountSvc.UpdateOrgListAsync(new V0UpdateOrgListRequest {
      AccountId = request.CreatedBy,
      OrgId = createdOrg.Id,
      Action = V0OrgListUpdateActions.Add
    });

    //イベントを伝搬させるのをEventBridgeに依頼
    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationInfoCreated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("info_id", new Value { StringValue = $"{org.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{org.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{org.Description}" });
    newEventData.Payload.Fields.Add("created_by", new Value { StringValue = $"{org.CreatedBy}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{org.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{org.UpdatedAt}" });
    newEventData.Payload.Fields.Add("deleted_at", new Value { StringValue = $"{org.DeletedAt}" });

    var createdEventData = _eventBridge.NewEvent(
      newEventData
    );

    // 作成された組織情報を gRPCレスポンスとして返却
    return new V0InfoChangesReply {
      EventId = createdEventData.EventId
    };
  }

  // 組織情報の更新処理
  // - 他組織との名前重複チェック（自身以外）
  // - 更新対象の存在確認とフィールド更新
  // - 更新後の最新データを返却
  public override async Task<V0InfoChangesReply> Update(V0UpdateOrganizationRequest request, ServerCallContext context) {
    // 名前重複チェック（自身以外のIDと重複していないか）
    var conflict = await _orgs.Find(x => x.Name == request.Name && x.Id != request.Id && x.DeletedAt == null).AnyAsync();
    if (conflict) {
      throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization name already exists"));
    }

    // 更新対象のフィールドを指定してUpdate定義
    var update = Builders<OrganizationEntity>.Update
      .Set(x => x.Name, request.Name)
      .Set(x => x.Description, request.Description)
      .Set(x => x.UpdatedAt, DateTime.UtcNow);

    // 更新実行（DeletedAtがnullのもののみ対象）
    var result = await _orgs.UpdateOneAsync(x => x.Id == request.Id && x.DeletedAt == null, update);
    if (result.MatchedCount == 0) {
      throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
    }

    // 更新後の最新データを取得
    var updated = await _orgs.Find(x => x.Id == request.Id).FirstOrDefaultAsync();

    //イベントを伝搬させるのをEventBridgeに依頼
    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationInfoUpdated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("info_id", new Value { StringValue = $"{updated.Id}" });
    newEventData.Payload.Fields.Add("name", new Value { StringValue = $"{updated.Name}" });
    newEventData.Payload.Fields.Add("description", new Value { StringValue = $"{updated.Description}" });
    newEventData.Payload.Fields.Add("created_by", new Value { StringValue = $"{updated.CreatedBy}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{updated.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{updated.UpdatedAt}" });
    newEventData.Payload.Fields.Add("deleted_at", new Value { StringValue = $"{updated.DeletedAt}" });

    var updatedEventData = _eventBridge.NewEvent(
      newEventData
    );

    // 更新結果をレスポンスとして返却
    return new V0InfoChangesReply {
      EventId = updatedEventData.EventId
    };
  }

  // 組織の論理削除処理
  // - 組織の存在確認
  // - DeletedAt フィールドの更新による論理削除
  // - 関連データ（メンバー・ロール・チャット）の物理削除
  public override async Task<V0InfoChangesReply> Delete(V0DeleteOrganizationRequest request, ServerCallContext context) {
    // 対象組織の存在確認
    var org = await _orgs.Find(x => x.Id == request.Id).FirstOrDefaultAsync();
    if (org == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
    }

    // DeletedAt を現在時刻に設定（論理削除）
    var update = Builders<OrganizationEntity>.Update.Set(x => x.DeletedAt, DateTime.UtcNow);
    await _orgs.UpdateOneAsync(x => x.Id == request.Id, update);

    //イベントを伝搬させるのをEventBridgeに依頼

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationInfoDeleted",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("info_id", new Value { StringValue = $"{request.Id}" });

    var deletedEventData = _eventBridge.NewEvent(
        newEventData
    );

    // 削除結果を返却
    return new V0InfoChangesReply {
      EventId = deletedEventData.EventId
    };
  }

  // 組織情報の取得処理
  // - 論理削除されていない組織を検索
  // - 該当組織が存在しない場合は NotFound を返却
  public override async Task<V0GetInfoOrganizationReply> GetInfo(V0GetInfoOrganizationRequest request, ServerCallContext context) {
    var org = await _orgs.Find(x => x.Id == request.Id && x.DeletedAt == null).FirstOrDefaultAsync();
    if (org == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
    }

    // 組織情報をレスポンスとして返却
    return new V0GetInfoOrganizationReply {
      Id = org.Id,
      Name = org.Name,
      Description = org.Description,
      CreatedBy = org.CreatedBy,
      CreatedAt = Timestamp.FromDateTime(org.CreatedAt),
      UpdatedAt = Timestamp.FromDateTime(org.UpdatedAt),
      DeletedAt = null // 論理削除されていないため null を明示
    };
  }
}
