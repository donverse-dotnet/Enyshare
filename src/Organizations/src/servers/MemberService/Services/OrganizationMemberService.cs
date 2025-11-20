using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MongoDB.Bson;
using Pocco.Libs.Protobufs.Types;
using MemberService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Enums;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;
using System.Data;
using Pocco.Libs.Protobufs.Services;


namespace MemberService.Services;

/// <summary>
/// gRPCサービスの実装クラス：メンバー情報の作成・更新・取得・削除を提供
/// MongoDBをバックエンドに使用し、論理削除とバージョン管理に対応
/// </summary>
public class OrganizationsMemberServiceImpl : V0OrganizationMemberService.V0OrganizationMemberServiceBase {
  private readonly IMemberRepository _repository;
  private readonly ILogger<OrganizationsMemberServiceImpl> _logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge;

  public OrganizationsMemberServiceImpl(
    [FromServices] IMemberRepository repository,
    [FromServices] ILogger<OrganizationsMemberServiceImpl> logger,
    [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge
  ) {
    _repository = repository;
    _logger = logger;
    _eventBridge = eventBridge;

    _logger.LogInformation("OrganizationsMemberServiceImpl is initialized!");
  }

  /// <summary>
  /// メンバーの新規登録処理
  /// - 組織IDとユーザーIDの重複チェック（論理処理されていないもののみ対象）
  /// - メンバーエンティティの作成とMongoDBへの保存
  /// - 登録されたメンバー情報をレスポンスとして返却
  /// </summary>
  public override async Task<V0MemberChangesReply> RequestToJoin(V0CreateMemberRequest request, ServerCallContext context) {
    // メンバーエンティティの作成
    var member = new MemberEntity {
      Id = ObjectId.GenerateNewId().ToString()
    };

    MemberEntity createdMember = await _repository.CreateAsync(request.OrganizationId, member);
    _logger.LogInformation("{MemberId} is successfully created on {OrganizationId}", createdMember.Id, request.OrganizationId);

    //　イベントを伝搬させるのをEventBridgeに依頼
    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationMemberCreated",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("id", new Value { StringValue = $"{request.Id}" });
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
  public override async Task<V0MemberChangesReply> UpdateOrgUser(V0UpdateMemberRequest request, ServerCallContext context) {
    // 更新結果をレスポンスとして返却
    var model = new MemberEntity {
      Id = request.Id,
      Nickname = request.Nickname,
    };

    var updated = await _repository.TryUpdateAsync(request.OrganizationId, request.Id, model);
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

    newEventData.Payload.Fields.Add("id", new Value { StringValue = $"{request.Id}" });
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
  public override async Task<V0MemberChangesReply> RequestToLeave(V0DeleteMemberRequest request, ServerCallContext context) {
    var deleted = await _repository.DeleteAsync(request.OrganizationId, request.Id);
    if (!deleted) {
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found or no fields to delete"));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationMemberDeleted",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.InvokedBy,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("id", new Value { StringValue = $"{request.Id}" });

    var deletedEventData = _eventBridge.NewEvent(newEventData);

    return new V0MemberChangesReply {
      EventId = deletedEventData.EventId
    };
  }

  /// <summary>
  /// メンバー情報の取得処理
  /// - 指定IDのメンバーを検索（論理削除されていないもののみ対象）
  /// - 該当メンバーが存在しない場合は NotFound を返却
  /// </summary>
  public override async Task<V0GetMemberReply> Get(V0GetMemberRequest request, ServerCallContext context) {
    var member = await _repository.GetByIdAsync(request.OrganizationId, request.Id);
    if (member == null) {
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));
    }

    // メンバー情報をレスポンスとして返却
    var model = new V0MemberModel {
      Id = request.Id,
      OrganizationId = request.OrganizationId,
    };

    return new V0GetMemberReply {
      Id = model.Id
    };
  }
  public override async Task<V0GetListReply> GetList(V0GetListRequest request, ServerCallContext context) {
    var members = await _repository.GetListAsync(request.OrganizationId);

    var response = new V0GetListReply();
    response.Members.AddRange(members.Select(m => new  MemberListResponse {
      Id = m.Id
    }));

    return response;
  }
}
