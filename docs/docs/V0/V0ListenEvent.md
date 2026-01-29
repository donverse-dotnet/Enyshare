# イベント購読メソッド

イベントを購読するメソッドです。この`Listen`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

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
- `stream V0EventData` V0EventTopics型のトピックとstring型のイベントタイプとstring型のapiバージョンとstring型のイベントidとgoogle.protobuf.Timestamp型のイベント発行日時とstring型のイベント発行者とgoogle.protobuf.Struct型のイベントペイロードを返します。

| データ | 変数名 |
|------------|-----|
| V0EventTopics | topic |
| string | event_type |
| string | api_version |
| string | event_id |
| google.protobuf.Timestamp | invoked_at |
| string | invoked_by |
| google.protobuf.Struct | payload |

```proto
service V0EventService{
    rpc Listen(ListenRequest) returns (stream V0EventData);
}
```