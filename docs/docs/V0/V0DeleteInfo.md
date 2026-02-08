# 情報削除メソッド

組織の情報を削除するメソッドです。この`Delete`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0BaseRequest` string型のidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | id |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc Delete(V0BaseRequest) returns (V0EventInvokedResponse);
}
```