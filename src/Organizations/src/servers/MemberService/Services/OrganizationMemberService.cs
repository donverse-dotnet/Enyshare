using Grpc.Core;
using MongoDB.Bson;
using Pocco.Libs.Protobufs.Types;
using MemberService.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace MemberService.Services;

/// <summary>
/// gRPCサービスの実装クラス：メンバー情報の作成・更新・取得・削除を提供
/// MongoDBをバックエンドに使用し、論理削除とバージョン管理に対応
/// </summary>
public class OrganizationsMemberServiceImpl : V0OrganizationMemberService.V0OrganizationMemberServiceBase {
  private readonly IMemberRepository _repository;
  private readonly ILogger<OrganizationsMemberServiceImpl> _logger;

  public OrganizationsMemberServiceImpl([FromServices] IMemberRepository repository,
[FromServices] ILogger<OrganizationsMemberServiceImpl> logger) {
    _repository = repository;
    _logger = logger;

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
      Id = ObjectId.GenerateNewId().ToString(),
      OrganizationId = request.OrganizationId
    };

    MemberEntity createdMember = await _repository.CreateAsync(request.OrganizationId, member);
    _logger.LogInformation("{MemberId} is successfully created on {OrganizationId}", createdMember.Id, request.OrganizationId);

    return new V0MemberChangesReply {
      EventId = "fake id" //TODO: eventbridgeからのidに置き換える
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
      OrganizationId = request.OrganizationId,
      Nickname = request.Nickname,
    };

    var updated = await _repository.TryUpdateAsync(request.OrganizationId, request.Id, model);
    if (updated == false) {
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found or no fields update"));
    }

    return new V0MemberChangesReply {
      EventId = "fake id" //TODO: eventbridgeからのidに置き換える
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

    return new V0MemberChangesReply {
      EventId = "fake id" //TODO: eventbridgeからのidに置き換える
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
      Member = model
    };
  }
}
