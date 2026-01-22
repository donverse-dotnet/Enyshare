# メンバー退出メソッド

メンバーを退出するメソッドです。この`LeaveMember`メソッドは、以下のリクエストを送り、以下のレスポンスを返します。

# リクエスト
- `V0LeaveMemberRequest` string型の組織idとstring型のユーザーidをリクエストします。

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | user_id |

# レスポンス
- `V0EventInvokedResponse` string型のイベントidを返します。

| データ | 変数名 |
|------------|-----|
| string | event_id |

```proto
service V0ApiService{
    rpc LeaveMember(V0LeaveMemberRequest) returns(V0EventInvokedResponse);
}
```