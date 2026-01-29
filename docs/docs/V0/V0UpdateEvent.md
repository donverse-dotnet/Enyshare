# 購読更新メソッド

購読を更新するメソッドです。この`UpdateListen`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `ListenRequest` string型のユーザーidとstring型のセッションidとrepeated V0EventTopics型のトピックスとrepeated string型の組織ids(リスト)とstring型の組織id(アクティブ)とrepeated string型のダイレクトメッセージids(リスト)をリクエストします。

| データ | 変数名 |
|------------|-----|
| string | user_id |
| string | session_id |
| repeated V0EventTopics | topics |
| repeated string | organization_ids |
| string | active_organization_id |
| repeated string | direct_message_ids |

# レスポンス
- `google.protobuf.Empty` 空を返します。

```proto
service V0EventService{
    rpc UpdateListen(ListenRequest) returns (google.protobuf.Empty);
}
```