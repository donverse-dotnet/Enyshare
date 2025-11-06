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

  [BsonElement("updateAt")]
  public DateTime UpdateAt { get; set; }

  public bool HasNickname => !string.IsNullOrWhiteSpace(Nickname);

  public bool IsNicknameChanged(string nickname) {
    return Nickname != nickname;
    }
}
