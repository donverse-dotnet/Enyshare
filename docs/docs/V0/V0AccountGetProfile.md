# アカウント取得メソッド

アカウントを取得するメソッドです。この`GetProfile`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

#　リクエスト
- `V0AccountGetProfileRequest` string型のユーザーidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | user_id |

# レスポンス
- `V0BaseAccount` string型のidとstring型のユーザー名とstring型のアバターurlとV0AccountRoles型のroleとV0AccountStatus型のstatusとgoogle.protobuf.Timestamp型のcreated_atとgoogle.protobuf.Timestamp型のupdated_atを返します。

| データ | 変数名 |
|------------|-----|
| string | id |
| string | user_name |
| string | avatar_url |
| V0AccountRoles | role |
| V0AccountStatus | status |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc GetProfile(V0AccountGetProfileRequest) returns (V0BaseAccount);
}
```