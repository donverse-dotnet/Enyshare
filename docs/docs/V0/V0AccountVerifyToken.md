# トークン確認メソッド

トークンを確認するためのメソッドです。この`VerifyToken`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `google.protobuf.Empty` 空をリクエストします。

# レスポンス
- `V0ApiSessionData` string型のセッションidとstring型のアカウントidとstring型のトークンとgoogle.protobuf.Timestamp型の有効期限とgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時を返します。

| データ | 変数名 |
|------------|-----|
| string | session_id |
| string | account_id |
| string | token |
| google.protobuf.Timestamp | expires_at |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc VerifyToken(google.protobuf.Empty) returns (V0ApiSessionData);
}
```