# セッション終了メソッド

セッションを終了するメソッドです。この`Unauthenticate`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `google.protobuf.Empty` 空をリクエストします。

# レスポンス 
- `google.protobuf.Empty` 空を返します。

```proto
service V0ApiService{
    rpc Unauthenticate(google.protobuf.Empty) returns (google.protobuf.Empty);
}
```