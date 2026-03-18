using Google.Protobuf.WellKnownTypes;

using MongoDB.Bson.Serialization.Attributes;

using Pocco.Libs.Protobufs.Auth.Types;

namespace Pocco.Srv.Auth.Models;

public class V0SessionDataWrapper {
  // セッションID
  [BsonId]
  [BsonElement("SessionId")]
  public string SessionId { get; set; } = string.Empty;
  // アカウントID
  [BsonElement("AccountId")]
  public string AccountId { get; set; } = string.Empty;
  // トークン
  [BsonElement("Token")]
  public string Token { get; set; } = string.Empty;
  // 有効期限（UNIXタイムスタンプ）
  [BsonElement("ExpiresAt")]
  public Timestamp? ExpiresAt { get; set; }
  // 作成日時（UNIXタイムスタンプ）
  [BsonElement("CreatedAt")]
  public Timestamp? CreatedAt { get; set; }
  // 更新日時（UNIXタイムスタンプ）
  [BsonElement("UpdatedAt")]
  public Timestamp? UpdatedAt { get; set; }

  public V0SessionData ToV0SessionData() {
    return new V0SessionData {
      SessionId = SessionId,
      AccountId = AccountId,
      Token = Token,
      ExpiresAt = ExpiresAt,
      CreatedAt = CreatedAt,
      UpdatedAt = UpdatedAt
    };
  }
}
