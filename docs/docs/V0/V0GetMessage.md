# メッセージ取得メソッド

メッセージを取得するメソッドです。この`GetMessage`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0GetOrDeleteMessageRequest` string型の組織idとstring型のチャットidとstring型のメッセージidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | chat_id |
| string | message_id |

# レスポンス
- `Message` string型のメッセージidとstring型の組織idとstring型のチャットidとstring型のセンダーid(ユーザーid)とstring型のコンテントとgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時を返します。

| データ | 変数名 |
|------------|-----|
| string | message_id |
| string | organization_id |
| string | chat_id |
| string | sender_id (user_id) |
| string | content |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc GetMessage(V0GetOrDeleteMessageRequest) returns (Message);
}
```