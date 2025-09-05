using MongoDB.Bson;
using MongoDb.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

// メンバー情報を保持するMongoDBドキュメント
public class MemberEntity
{
  [BsonId]
  [BsonRepresentation(BsonType.String)]
  public string Id { get; set; } // ユーザーID（gRPCと同期）

  [BsonElement("orgId")]
  public string OrgId { get; set; } // 所属組織ID

  [BsonElement("nickname")]
  public string Nickname { get; set; } // 表示名

  [BsonElement("roles")]
  public List<string> Roles { get; set; } = new(); // 権限ロール（例：admin, member)

  [BsonElement("joinAt")]
  public DateTime JoinAt { get; set; } // 参加日時（UTC)

  [BsonElement("isAvtive")]
  public bool isAvtive { get; set; } = true; // 有効状態（大会時 false）
}
