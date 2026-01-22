# ロール更新メソッド

ロールを更新するメソッドです。この`UpdateRole`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `Role` string型のロールidとstring型の組織idとstring型の名前とrepeated string型のpermissionsとgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時をリクエストします。

| データ | 変数名 |
|------------|-----|
| string | role_id |
| string | organization_id |
| string | name |
| repeated string | permissions |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc UpdateRole(Role) returns(V0EventInvokedResponse);
}
```