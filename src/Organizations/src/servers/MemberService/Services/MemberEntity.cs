#pragma warning disable CS8618
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// メンバー情報を保持するMongoDBドキュメント
public class MemberEntity {
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } = string.Empty; // ユーザーID（gRPCと同期）

  [BsonElement("nickname")]
  public string Nickname { get; set; } = string.Empty;// 表示名

  [BsonElement("name")]
  public string Name { get; set; } = string.Empty;

  [BsonElement("roles")]
  public List<string> Roles { get; set; } = new(); // 権限ロール（例：admin, member)

  [BsonElement("joinedAt")]
  public DateTime JoinedAt { get; set; } = DateTime.UtcNow;// 参加日時（UTC)

  public bool IsNicknameChanged(string nickname) {
    return Nickname != nickname;
    }
}
