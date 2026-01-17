# メンバー取得メソッド

メンバーを取得するメソッドです。この`GetMember`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0GetOrDeleteXRequest` string型の組織idとstring型のidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | id |

# レスポンス
- `Member` string型のメンバーidとstring型のユーザーidとstring型の組織idとrepeated string型のrolesとgoogle.protobuf.Timestamp型の参加日時とgoogle.protobuf.Timestamp型の更新日時を返します。

| データ | 変数名 |
|------------|-----|
| string | member_id |
| string | user_id |
| string | organization_id |
| repeated string | roles |
| google.protobuf.Timestamp | joined_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc GetMember (V0GetOrDeleteXRequest) returns (Member);
}
```