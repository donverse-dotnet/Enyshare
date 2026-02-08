# アカウント登録メソッド

アカウントを登録するメソッドです。この`Register`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0AccountRegisterRequest`　string型のメールアドレスとstring型のパスワードをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | Email |
| string | password |



# レスポンス
- `google.protobuf.Empty` 
空を返します。

```proto
service V0ApiService{
    rpc Register(V0AccountRegisterRequest) returns (google.protobuf.Empty);
}
```