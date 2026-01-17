# 情報作成メソッド

組織の情報を作成するメソッドです。この`Create`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0CreateOrganizationRequest` V0CreateXRequest型のベースとstring型の説明をリクエストします。

| データ | 変数名 |
|------------|-----|
| V0CreateXRequest | base |
| string | description |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc Create(V0CreateOrganizationRequest) returns (V0EventInvokedResponse);
}
```