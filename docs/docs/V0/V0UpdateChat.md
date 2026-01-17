# チャット更新メソッド

チャットを更新するメソッドです。この`UpdateChat`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0UpdateChatRequest` string型の組織idとstring型のチャットidとstring型の名前とstring型の説明をリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | chat_id |
| string | name |
| string | description |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc UpdateChat (V0UpdateChatRequest) returns (V0EventInvokedResponse);
}
```