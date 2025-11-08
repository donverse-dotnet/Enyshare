using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// メンバー情報を保持するMongoDBドキュメント
public class MemberEntity {
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } // ユーザーID（gRPCと同期）

  [BsonElement("nickname")]
  public string Nickname { get; set; } // 表示名

  [BsonElement("roles")]
  public List<string> Roles { get; set; } = new(); // 権限ロール（例：admin, member)

  [BsonElement("joinedAt")]
  public DateTime JoinedAt { get; set; } // 参加日時（UTC)

  public bool IsNicknameChanged(string nickname) {
    return Nickname != nickname;
    }
}
