# ロールリスト取得メソッド

ロールのリストを取得するメソッドです。この`ListRoles`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0ListXRequest` V0BaseRequest型のbaseとint32型のページサイズとint32型のページナンバーをリクエストします。

| データ | 変数名 |
|------------|-----|
| V0BaseRequest | base |
| int32 | page_size |
| int32 | page_number |

# レスポンス
- `V0ListRolesResponse` repeated Role型のrolesを返します。

| データ | 変数名 |
|------------|-----|
| repeated Role | roles |

```proto
service V0ApiService{
    rpc ListRoles(V0ListXRequest) returns (V0ListRolesResponse);
}
```