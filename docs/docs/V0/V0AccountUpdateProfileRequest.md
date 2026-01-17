# V0AccountUpdateProfileRequest

### UpdateProfileメソッドのリクエスト型
- **リクエスト名**: V0AccountUpdateProfileRequest
- **タイプ**: Unary RPC
- **説明**: ユーザーID、ユーザー名、アバターURL、メールアドレス、通知設定、ステータス、パスワード(ハッシュ化済み)をリクエストします。

---
| データ | 変数名 |
|------------|-----|
| string | user_id |
| string | username |
| string | avatar_url |
| string | email |
| V0AccountNotificationSettings | notification_settings |
| string | password |