using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Organizations_Member.Types;
using MemberService.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Pocco.Libs.Protobufs.Organizations_Member.Services;
using Pocco.Libs.Protobufs.EventBridge.Services;
using Pocco.Libs.Protobufs.EventBridge.Types;
using Pocco.Libs.Protobufs.EventBridge.Enums;
using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.Accounts.Types;
using Pocco.Libs.Protobufs.Accounts.Enums;
using MongoDB.Bson;
using MongoDB.Driver;


namespace MemberService.Services;

/// <summary>
/// gRPCサービスの実装クラス：メンバー情報の作成・更新・取得・削除を提供
/// MongoDBをバックエンドに使用し、論理削除とバージョン管理に対応
/// </summary>
public class OrganizationsMemberServiceImpl : V0OrganizationMemberService.V0OrganizationMemberServiceBase {
  private readonly IMemberRepository _repository;
  private readonly ILogger<OrganizationsMemberServiceImpl> _logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;
  private readonly V0InternalAccountService.V0InternalAccountServiceClient _internalAccountSvc;

  public OrganizationsMemberServiceImpl(
    [FromServices] IMemberRepository repository,
    [FromServices] ILogger<OrganizationsMemberServiceImpl> logger,
    [FromServices] V0InternalAccountService.V0InternalAccountServiceClient internalAccountSvc,
    [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge
  ) {
    _repository = repository;
    _logger = logger;
    _internalAccountSvc = internalAccountSvc;
    _eventBridge = eventBridge;

    _logger.LogInformation("OrganizationsMemberServiceImpl is initialized!");
  }


  /// <summary>
  /// メンバー情報の取得処理
  /// - 指定IDのメンバーを検索（論理削除されていないもののみ対象）
  /// - 該当メンバーが存在しない場合は NotFound を返却
  /// </summary>
  public override async Task<V0MemberModel> Get(V0GetRequest request, ServerCallContext context) {
    var member = await _repository.GetByIdAsync(request.OrganizationId, request.MemberId);
    if (member == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));
    }

    // メンバー情報をレスポンスとして返却
    var model = new V0MemberModel {
      Id = member.Id,
      JoinedAt = Timestamp.FromDateTime(member.JoinedAt.ToUniversalTime()),
    };
    model.Roles.AddRange(member.Roles);

    return model;
  }

  /// <summary>
  /// メンバー一覧の取得処理
  /// - 指定組織IDに紐づくメンバーを全件取得（論理削除されていないもののみ対象）
  /// - 取得したメンバー情報をレスポンスとして返却
  /// </summary>
  /// <param name="request"></param>
  /// <param name="context"></param>
  /// <returns></returns>
  public override async Task<V0GetListResponse> List(V0GetListRequest request, ServerCallContext context) {
    var members = await _repository.GetListAsync(request.OrganizationId);

    var response = new V0GetListResponse();
    response.Members.AddRange(members.Select(member => new V0MemberModel {
      Id = member.Id,
      JoinedAt = Timestamp.FromDateTime(member.JoinedAt.ToUniversalTime()),
      Roles = { member.Roles }
    }));

    return response;
  }

  /// <summary>
  /// メンバーの新規登録処理
  /// - 組織IDとユーザーIDの重複チェック（論理処理されていないもののみ対象）
  /// - メンバーエンティティの作成とMongoDBへの保存
  /// - 登録されたメンバー情報をレスポンスとして返却
  /// </summary>
  public override async Task<V0MemberChangesReply> Join(V0JoinRequest request, ServerCallContext context) {

    var account = await _internalAccountSvc.GetAccountInfoAsync(new V0GetAccountRequest {
        Id = request.UserId
    });

    // メンバーエンティティの作成
    var member = new MemberEntity {
      Id = request.UserId,
      Nickname = account.Username,
      JoinedAt = DateTime.UtcNow,
      // TODO: Nicknameのアカウントサービスからの取得
      // TODO: デフォルトのRole設定のルール策定
    };

    var createdMember = await _repository.CreateAsync(request.OrganizationId, member);
    _logger.LogInformation("{MemberId} is successfully created on {OrganizationId}", createdMember.Id, request.OrganizationId);

    // var createdOrg = _orgs.FindAsync(item => item.Nickname == account.Username).Result.ToListAsync().Result.FirstOrDefault() ?? throw new RpcException(new Status(StatusCode.NotFound, "Organization mayde created but can't found."));

    //アカウントを更新
    var reply = await _internalAccountSvc.UpdateOrgListAsync(new V0UpdateOrgListRequest {
        AccountId = request.UserId,
        OrgId = request.OrganizationId,
        Action = V0OrgListUpdateActions.Add
    });

    //　イベントを伝搬させるのをEventBridgeに依頼
    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationMemberCreated",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("id", new Value { StringValue = $"{request.UserId}" });
    newEventData.Payload.Fields.Add("nickname", new Value { StringValue = $"{createdMember.Nickname}" });
    newEventData.Payload.Fields.Add("roles", new Value { StringValue = $"{createdMember.Roles}" });
    newEventData.Payload.Fields.Add("joined_at", new Value { StringValue = $"{createdMember.JoinedAt}" });

    var createdEventData = _eventBridge.NewEvent(newEventData);

    return new V0MemberChangesReply {
      EventId = createdEventData.EventId
    };
  }

  /// <summary>
  /// メンバー情報の更新処理
  /// - 指定IDのメンバーが存在するか確認（論理削除されていないもののみ対象）
  /// - RoleとUpdatedAtを更新
  /// - 更新後のメンバー情報をレスポンスとして返却
  /// </summary>
  public override async Task<V0MemberChangesReply> Update(V0UpdateRequest request, ServerCallContext context) {
    // 更新結果をレスポンスとして返却
    var model = new MemberEntity {
      Id = request.Member.Id,
      Roles = request.Member.Roles.ToList()
    };

    var updated = await _repository.TryUpdateAsync(request.OrganizationId, request.MemberId, model);
    if (updated == false) {
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found or no fields update"));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationMemberUpdated",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("id", new Value { StringValue = $"{request.MemberId}" });
    newEventData.Payload.Fields.Add("nickname", new Value { StringValue = $"{model.Nickname}" });
    newEventData.Payload.Fields.Add("roles", new Value { StringValue = $"{model.Roles}" });
    newEventData.Payload.Fields.Add("joined_at", new Value { StringValue = $"{model.JoinedAt}" });

    var updatedEventData = _eventBridge.NewEvent(newEventData);

    return new V0MemberChangesReply {
      EventId = updatedEventData.EventId
    };
  }

  /// <summary>
  /// メンバーの論理削除処理
  /// - 指定IDのメンバーの DeletedAt を現在時刻に設定
  /// - 該当メンバーが存在しない場合は NotFound を返却
  /// </summary>
  public override async Task<V0MemberChangesReply> Leave(V0LeaveRequest request, ServerCallContext context) {
    var deleted = await _repository.DeleteAsync(request.OrganizationId, request.MemberId);
    if (!deleted) {
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found or no fields to delete"));
    }

    var account = await _internalAccountSvc.GetAccountInfoAsync(new V0GetAccountRequest {
        Id = request.MemberId
    });

    //アカウントを更新
    var reply = await _internalAccountSvc.UpdateOrgListAsync(new V0UpdateOrgListRequest {
        AccountId = request.MemberId,
        OrgId = request.OrganizationId,
        Action = V0OrgListUpdateActions.Remove
    });

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationMemberDeleted",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("id", new Value { StringValue = $"{request.MemberId}" });

    var deletedEventData = _eventBridge.NewEvent(newEventData);

    return new V0MemberChangesReply {
      EventId = deletedEventData.EventId
    };
  }
}
