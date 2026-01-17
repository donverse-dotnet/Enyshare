# チャット取得メソッド

チャットを取得するメソッドです。この`GetChat`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0GetOrDeleteXRequest` string型の組織idとstring型のidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | id |

# レスポンス
- `Chat` string型のチャットidとstring型の組織idとstring型の名前とstring型の説明とstring型のタイプとgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時を返します。

| データ | 変数名 |
|------------|-----|
| string | chat_id |
| string | organization_id |
| string | name |
| string | description |
| string | type |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc GetChat (V0GetOrDeleteXRequest) returns (Chat);
}
```