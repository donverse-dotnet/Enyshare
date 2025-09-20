using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemberService.Services;
using Pocco.Libs.Protobufs.Types;


namespace MemberService.Services;

/// <summary>
/// gRPCサービスの実装クラス：メンバー情報の作成・更新・取得・削除を提供
/// MongoDBをバックエンドに使用し、論理削除とバージョン管理に対応
/// </summary>
public class OrganizationsMemberServiceImpl : V0OrganizationMemberService.V0OrganizationMemberServiceBase {
  // MongoDBのメンバーコレクション
  private readonly IMongoCollection<MemberEntity> _members;

  /// <summary>
  /// コンストラクタ：MongoDBインスタンスからメンバーコレクションを取得
  /// </summary>
  public OrganizationsMemberServiceImpl(IMongoDatabase mongo) {
    _members = mongo.GetCollection<MemberEntity>("members");
  }

  /// <summary>
  /// メンバーの新規登録処理
  /// - 組織IDとユーザーIDの重複チェック（論理処理されていないもののみ対象）
  /// - メンバーエンティティの作成とMongoDBへの保存
  /// - 登録されたメンバー情報をレスポンスとして返却
  /// </summary>
  public override async Task<V0CreateMemberReply> RequestToJoin(V0CreateMemberRequest request, ServerCallContext context) {
    // 重複チェック：同じ組織・ユーザーIDで論理削除されていないメンバーが存在するか
    var exists = await _members.Find(x =>
    x.OrganizationId == request.OrganizationId &&
    x.UserId == request.UserId &&
    x.DeletedAt == null).AnyAsync();

    if (exists) {
      // 重複がある場合はgRPCのAlreadyExistsステータスを返す
      throw new RpcException(new Status(StatusCode.AlreadyExists, "Member already exists"));
    }

    // メンバーエンティティの作成
    var member = new MemberEntity {
      Id = ObjectId.GenerateNewId().ToString(),
      OrganizationId = request.OrganizationId,
      UserId = request.UserId,
      Role = request.Role.ToList(),
      JoinedAt = DateTime.UtcNow,
      DeletedAt = null
    };

    // MongoDBに保存
    await _members.InsertOneAsync(member);

    // 登録されたメンバー情報をレスポンスとして返却
    var model = new V0MemberModel {
      Id = member.Id,
      OrganizationId = member.OrganizationId,
      UserId = member.UserId,
      JoinedAt = Timestamp.FromDateTime(member.JoinedAt.ToUniversalTime()),
      DeletedAt = null
    };

    model.Role.AddRange(member.Role);

    return new V0CreateMemberReply {
      Member = model
    };
  }

  /// <summary>
  /// メンバー情報の更新処理
  /// - 指定IDのメンバーが存在するか確認（論理削除されていないもののみ対象）
  /// - RoleとUpdatedAtを更新
  /// - 更新後のメンバー情報をレスポンスとして返却
  /// </summary>
  public override async Task<V0UpdateMemberReply> UpdateOrgUser(V0UpdateMemberRequest request, ServerCallContext context) {
    // 更新内容の定義（RoleとUpdatedAt）
    var updateDefinition = Builders<MemberEntity>.Update
    .Set(x => x.Role, request.Role.ToList())
    .Set(x => x.UpdateAt, DateTime.UtcNow);

    // 更新実行（DeletedAtがnullのもののみ対象）
    var updateResult = await _members.UpdateOneAsync(
      x => x.Id == request.Id && x.DeletedAt == null,
      updateDefinition);

    if (updateResult.MatchedCount == 0) {
      // 該当メンバーが存在しない場合はNotFoundを返す
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));
    }

    // 更新後の最新データを取得
    var updated = await _members.Find(x => x.Id == request.Id).FirstOrDefaultAsync();

    // 更新結果をレスポンスとして返却
    var model = new V0MemberModel {
      Id = updated.Id,
      OrganizationId = updated.OrganizationId,
      UserId = updated.UserId,
      JoinedAt = Timestamp.FromDateTime(updated.JoinedAt.ToUniversalTime()),
      DeletedAt = updated.DeletedAt.HasValue
        ? Timestamp.FromDateTime(updated.DeletedAt.Value.ToUniversalTime())
        : null
    };

    model.Role.AddRange(updated.Role);

    return new V0UpdateMemberReply {
      Member = model
    };
  }

  /// <summary>
  /// メンバーの論理削除処理
  /// - 指定IDのメンバーの DeletedAt を現在時刻に設定
  /// - 該当メンバーが存在しない場合は NotFound を返却
  /// </summary>
  public override async Task<V0DeleteMemberReply> RequestToLeave(V0DeleteMemberRequest request, ServerCallContext context) {
    // DeletedAtを現在時刻に設定（論理削除）
    var update = Builders<MemberEntity>.Update.Set(x => x.DeletedAt, DateTime.UtcNow);
    var result = await _members.UpdateOneAsync(x => x.Id == request.Id, update);

    if (result.MatchedCount == 0) {
      // 該当メンバーが存在しない場合はNotFoundを返す
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));
    }

    // 削除成功レスポンスを返却
    return new V0DeleteMemberReply {
      Success = true,
      Message = "Member deleted successfully"
    };
  }

  /// <summary>
  /// メンバー情報の取得処理
  /// - 指定IDのメンバーを検索（論理削除されていないもののみ対象）
  /// - 該当メンバーが存在しない場合は NotFound を返却
  /// </summary>
  public override async Task<V0GetMemberReply> Get(V0GetMemberRequest request, ServerCallContext context) {
    // メンバー検索（DeletedAtがnullのもののみ対象）
    var member = await _members.Find(x => x.Id == request.Id && x.DeletedAt == null).FirstOrDefaultAsync();

    if (member == null) {
      // 該当メンバーが存在しない場合はNotFoundを返す
      throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));
    }

    // メンバー情報をレスポンスとして返却
    var model = new V0MemberModel {
      Id = member.Id,
      OrganizationId = member.OrganizationId,
      UserId = member.UserId,
      JoinedAt = Timestamp.FromDateTime(member.JoinedAt.ToUniversalTime()),
      DeletedAt = null
    };

    model.Role.AddRange(member.Role);

    return new V0GetMemberReply {
      Member = model
    };
  }
}
