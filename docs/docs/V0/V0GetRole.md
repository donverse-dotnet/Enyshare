# ロール取得メソッド

ロールを取得するメソッドです。この`GetRole`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0GetOrDeleteXRequest` string型の組織idとstring型のidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | id |

# レスポンス
- `Role` string型のロールidとstring型の組織idとstring型の名前とrepeated string型のpermissionsとgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時を返します。

| データ | 変数名 |
|------------|-----|
| string | role_id |
| string | organization_id |
| string | name |
| repeated string | permissions |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc GetRole(V0GetOrDeleteXRequest) returns (Role);
}
```