# メンバー参加メソッド

メンバーに参加するメソッドです。この`JoinMember`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0JoinMemberRequest` string型の組織idとstring型のユーザーidとstring型の招待コード(privateのみ)をリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | user_id |
| string | invite_code |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc JoinMember(V0JoinMemberRequest) returns(V0EventInvokedResponse);
}
```