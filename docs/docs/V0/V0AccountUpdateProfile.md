# アカウント更新メソッド

アカウントを更新するメソッドです。この`UpdateProfile`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

#　リクエスト
- `V0AccountUpdateProfileRequest` string型のユーザーidとstring型のユーザー名とstring型のアバターurlとstring型のメールアドレスとV0AccountNotificationSettings型のnotification_settingsとstring型のパスワードをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | user_id |
| string | username |
| string | avatar_url |
| string | email |
| V0AccountNotificationSettings | notification_settings |
| string | password |

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
    UpdateProfile(V0AccountUpdateProfileRequest) returns (V0BaseAccount);
}
```