# 購読停止メソッド

購読を停止するメソッドです。この`Unlisten`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `UnlistenRequest` string型のユーザーidとstring型のセッションidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | user_id |
| string | session_id |

# レスポンス
- `google.protobuf.Empty` 空を返します。

```proto
service V0EventService{
    rpc Unlisten(UnlistenRequest) returns (google.protobuf.Empty);
}
```