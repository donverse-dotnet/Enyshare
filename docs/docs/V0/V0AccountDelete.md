# アカウント削除メソッド

アカウントを削除するメソッドです。この`DeleteAccount`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `google.protobuf.Empty` 空をリクエストします。

# レスポンス
- `google.protobuf.Empty` 空を返します。

```proto
service V0ApiService{
    rpc DeleteAccount(google.protobuf.Empty) returns (google.protobuf.Empty);
}
```