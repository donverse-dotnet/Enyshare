# チャットリスト取得メソッド

チャットのリストを取得するメソッドです。この`ListChats`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0ListXRequest` V0BaseRequest型のbaseとint32型のページサイズとint32型のページナンバーをリクエストします。

| データ | 変数名 |
|------------|-----|
| V0BaseRequest | base |
| int32 | page_size |
| int32 | page_number |

# レスポンス
- `V0ListChatsResponse` repeated Chat型のchatsを返します。

| データ | 変数名 |
|------------|-----|
| repeated Chat | chats |

```proto
service V0ApiService{
    rpc ListChats (V0ListXRequest) returns (V0ListChatsResponse);
}
```