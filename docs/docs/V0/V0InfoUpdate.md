# 情報更新メソッド

組織の情報を更新するメソッドです。この`UpdateOrganization`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0UpdateOrganizationRequest` string型の組織idとstring型の名前をリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | name |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc UpdateOrganization(V0UpdateOrganizationRequest) returns(V0EventInvokedResponse);
}
```