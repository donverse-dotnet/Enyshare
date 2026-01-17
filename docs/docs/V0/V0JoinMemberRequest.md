# V0JoinMemberRequest

### JoinMemberメソッドのリクエスト型
- **リクエスト名**: V0JoinMemberRequest
- **タイプ**: Unary RPC
- **説明**: 組織ID、ユーザーID、招待コード(privateのみ)をリクエストします。

---
| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | user_id |
| string | invite_code |
