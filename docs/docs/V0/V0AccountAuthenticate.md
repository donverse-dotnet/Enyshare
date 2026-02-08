# アカウント認証認可メソッド

アカウントを認証認可するためのメソッドです。この`Authenticate`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0AccountAuthenticateRequest` string型のメールアドレスとstring型のパスワードをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | Email |
| string | password |

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
    rpc Authenticate(V0AccountAuthenticateRequest) returns (V0ApiSessionData);
}
```