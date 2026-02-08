# ロール作成メソッド

ロールを作成するメソッドです。この`CreateRole`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0CreateRoleRequest` V0CreateXRequest型のベースとstring型の組織idをリクエストします。

| データ | 変数名 |
|------------|-----|
| V0CreateXRequest | base |
| string | organization_id |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc CreateRole(V0CreateRoleRequest) returns (V0EventInvokedResponse);
}
```