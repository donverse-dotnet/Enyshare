# メッセージ作成メソッド

メッセージを作成するメソッドです。この`CreateMessage`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `Message` string型のメッセージidとstring型の組織idとstring型のチャットidとstring型のセンダーid(ユーザーid)とstring型のコンテントとgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時をリクエストします。

| データ | 変数名 |
|------------|-----|
| string | message_id |
| string | organization_id |
| string | chat_id |
| string | sender_id (user_id) |
| string | content |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc CreateMessage(Message) returns(V0EventInvokedResponse);
}
```