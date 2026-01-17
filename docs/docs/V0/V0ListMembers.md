# メンバーリスト取得メソッド

メンバーのリストを取得するメソッドです。この`ListMembers`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0ListXRequest` V0BaseRequest型のbaseとint32型のページサイズとint32型のページナンバーをリクエストします。

| データ | 変数名 |
|------------|-----|
| V0BaseRequest | base |
| int32 | page_size |
| int32 | page_number |

# レスポンス
- `V0ListMembersResponse` repeated Member型のmembersを返します。

| データ | 変数名 |
|------------|-----|
| repeated Member | members |

```proto
service V0ApiService{
    rpc ListMembers(V0ListXRequest) returns (V0ListMembersResponse);
}
```