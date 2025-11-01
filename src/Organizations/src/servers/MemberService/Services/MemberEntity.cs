using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// メンバー情報を保持するMongoDBドキュメント
public class MemberEntity {
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } // ユーザーID（gRPCと同期）

  [BsonElement("orgId")]
  public string OrganizationId { get; set; } // 所属組織ID

  [BsonElement("nickname")]
  public string Nickname { get; set; } // 表示名

  [BsonElement("role")]
  public List<string> Role { get; set; } = new(); // 権限ロール（例：admin, member)

  [BsonElement("joinedAt")]
  public DateTime JoinedAt { get; set; } // 参加日時（UTC)

  [BsonElement("isAvtive")]
  public bool isAvtive { get; set; } = true; // 有効状態（大会時 false）

  [BsonElement("userId")]
  public string UserId { get; set; }

  [BsonElement("deleteAt")]
  public DateTime? DeletedAt { get; set; }

  [BsonElement("updateAt")]
  public DateTime UpdateAt { get; set; }

  [BsonElement("is_private")]
  public bool Is_Private { get; set; } = false;

  public bool HasNickname => !string.IsNullOrWhiteSpace(Nickname);

  public bool IsNicknameChanged(string nickname) {
    return Nickname != nickname;
    }
}
