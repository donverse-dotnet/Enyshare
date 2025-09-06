using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

// 組織情報を保持するMongoDBドキュメント
public class OrganizationEntity
{
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } // 組織ID

  [BsonElement("name")]
  public string Name { get; set; } // 組織名

  [BsonElement("inviteCode")]
  public string InviteCode { get; set; } // 招待コード（メンバー参加時に使用）
}
