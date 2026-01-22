# メッセージリスト取得メソッド

メッセージのリストを取得するメソッドです。この`ListMessages`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0ListMessagesRequest` string型のチャットidとint32型のページサイズとint32型のページナンバーをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | chat_id |
| int32 | page_size |
| int32 | page_number |

# レスポンス
- `V0ListMessagesResponse` repeated Message型のmessagesを返します。

| データ | 変数名 |
|------------|-----|
| repeated Message | messages |

```proto
service V0ApiService{
    rpc ListMessages(V0ListMessagesRequest) returns (V0ListMessagesResponse);
}
```