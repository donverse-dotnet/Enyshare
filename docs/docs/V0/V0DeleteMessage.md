# メッセージ削除メソッド

メッセージを削除するメソッドです。この`DeleteMessage`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0GetOrDeleteMessageRequest` string型の組織idとstring型のチャットidとstring型のメッセージidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | chat_id |
| string | message_id |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc DeleteMessage(V0GetOrDeleteMessageRequest) returns(V0EventInvokedResponse);
}
```