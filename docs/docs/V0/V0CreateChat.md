# チャット作成メソッド

チャットを作成するメソッドです。この`CreateChat`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0CreateChatRequest` V0CreateXRequest型のベースとstring型の組織idとstring型のタイプをリクエストします。

| データ | 変数名 |
|------------|-----|
| V0CreateXRequest | base |
| string | organization_id |
| string | type |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc CreateChat (V0CreateChatRequest) returns (V0EventInvokedResponse);
}
```