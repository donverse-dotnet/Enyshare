# Role

### UpdateRoleメソッドのリクエスト型
---
### GetRoleメソッドのレスポンス型
---

- **リクエスト名**: Role
- **タイプ**: Unary RPC
- **リクエスト型説明**: ロールID、組織ID、ユーザー名、権限、作成日時、更新日時をリクエストします。
- **レスポンス型説明**: ロールID、組織ID、ユーザー名、権限、作成日時、更新日時を返します。

---
| データ | 変数名 |
|------------|-----|
| string | role_id |
| string | organization_id |
| string | name |
| repeated string | permissions |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

