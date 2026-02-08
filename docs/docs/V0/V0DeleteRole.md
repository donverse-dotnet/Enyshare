# ロール削除メソッド

ロールを削除するメソッドです。この`DeleteRole`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0GetOrDeleteXRequest` string型の組織idとstring型のidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | id |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc DeleteRole(V0GetOrDeleteXRequest) returns(V0EventInvokedResponse);
}
```