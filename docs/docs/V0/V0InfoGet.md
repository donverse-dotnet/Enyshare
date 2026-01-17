# 情報取得メソッド

組織の情報を取得するメソッドです。この`Get`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0BaseRequest` string型のidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | id |

# レスポンス
- `Organization` string型の組織idとstring型の名前とstring型の説明とstring型の作成者とgoogle.protobuf.Timestamp型の作成日時とgoogle.protobuf.Timestamp型の更新日時を返します。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | name |
| string | description |
| string | created_by (user_id) |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

```proto
service V0ApiService{
    rpc Get (V0BaseRequest) returns (Organization);
}
```