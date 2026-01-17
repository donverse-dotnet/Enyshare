# Message

### CreateMessageメソッドのリクエスト型
---
### UpdateMessageメソッドのリクエスト型
---
### GetMessageメソッドのレスポンス型
---

- **リクエスト名**: Message
- **タイプ**: Unary RPC
- **リクエスト型説明**: メッセージID、組織ID、チャットID、センダーID(user_id)、コンテント、作成日時、更新日時をリクエストします。
- **レスポンス型説明**: メッセージID、組織ID、チャットID、センダーID(user_id)、コンテント、作成日時、更新日時を返します。

---
| データ | 変数名 |
|------------|-----|
| string | message_id |
| string | organization_id |
| string | chat_id |
| string | sender_id |
| string | content |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |