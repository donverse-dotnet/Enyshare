# 補足

メソッドの説明に、同じリクエストと同じレスポンスがありますが、以下のリクエストと以下のレスポンスは同じデータをリクエストし、同じデータを返します。

---
- `google.protobuf.Empty`

- リクエスト

| メソッド |
|------------|
| DeleteAccount |
| Unauthenticate |
| VerifyToken |

- レスポンス

| メソッド |
|------------|
| Register |
| DeleteAccount |
| Unauthenticate |

---

- `V0BaseRequest`

| データ | 変数名 |
|------------|-----|
| string | id |

- リクエスト

| メソッド |
|------------|
| Get |
| Delete |

---

- `V0ListXRequest`

| データ | 変数名 |
|------------|-----|
| V0BaseRequest | base |
| int32 | page_size |
| int32 | page_number |

| メソッド |
|------------|
| ListMembers |
| ListRoles |
| ListChats |

---

- `V0GetOrDeleteXRequest`

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | id |

| メソッド |
|------------|
| GetMember |
| GetRole |
| DeleteRole |
| GetChat |
| DeleteChat |

---

- `Role`

| データ | 変数名 |
|------------|-----|
| string | role_id |
| string | organization_id |
| string | name |
| repeated string | permissions |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

- リクエスト

| メソッド |
|------------|
| UpdateRole |

- レスポンス

| メソッド |
|------------|
| GetRole |

---

- `V0GetOrDeleteMessageRequest`

| データ | 変数名 |
|------------|-----|
| string | organization_id |
| string | chat_id |
| string | message_id |

- リクエスト

| メソッド |
|------------|
| GetMessage |
| DeleteMessage |

---

- `Message`

| データ | 変数名 |
|------------|-----|
| string | message_id |
| string | organization_id |
| string | chat_id |
| string | sender_id (user_id) |
| string | content |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

- リクエスト

| メソッド |
|------------|
| CreateMessage |
| UpdateMessage |

- レスポンス

| メソッド |
|------------|
| Message |

---

- `V0ApiSessionData`

| データ | 変数名 |
|------------|-----|
| string | session_id |
| string | account_id |
| string | token |
| google.protobuf.Timestamp | expires_at |
| google.protobuf.Timestamp | created_at |
| google.protobuf.Timestamp | updated_at |

- レスポンス

| メソッド |
|------------|
| Authenticate |
| VerifyToken |

---

- `V0EventInvokedResponse`

| データ | 変数名 |
|------------|-----|
| string | event_id |

- レスポンス

| メソッド |
|------------|
| Create |
| UpdateOrganization |
| Delete |
| JoinMember |
| LeaveMember |
| CreateRole |
| UpdateRole |
| DeleteRole |
| CreateChat |
| UpdateChat |
| DeleteChat |
| CreateMessage |
| UpdateMessage |
| DeleteMessage |

---
