# 1. サービス一覧

| サービス名  | 説明                                          |
|--------------|--------------------------------------------------|
| V0ApiService | ユーザーのアカウントを登録するAPI gRPCサービスです。|
| V0ApiService | ユーザーのアカウントを認証するAPI gRPCサービスです。|
| V0ApiService | 組織のチャットを管理するAPI gRPCサービスです。|
| V0ApiService | 組織の情報を管理するAPI gRPCサービスです。|
| V0ApiService | 組織のメンバーを管理するAPI gRPCサービスです。|
| V0ApiService | 組織のメッセージを管理するAPI gRPCサービスです。|
| V0ApiService | 組織のロールを管理するAPI gRPCサービスです。|
| V0EventsService | イベント購読を管理するEvent gRPCサービスです。|

# 2. ApiService
### Accounts
    rpc Register(V0AccountRegisterRequest) returns (google.protobuf.Empty);
    rpc GetProfile(V0AccountGetProfileRequest) returns (V0BaseAccount);
    rpc UpdateProfile(V0AccountUpdateProfileRequest) returns (V0BaseAccount);
    rpc DeleteAccount(google.protobuf.Empty) returns (google.protobuf.Empty);

| RPC名  | 説明| 認証|
|--------|----|-----|
| Register      | ユーザーのメールアドレスとパスワードを利用してアカウントを登録するためのメソッド。| 不要 |
| GetProfile    | ユーザーのIDを利用してアカウントの情報を取得するためのメソッド。| 必要 |
| UpdateProfile | ユーザーID、更新するユーザー名、更新するアバターURL、更新するメールアドレス、更新する通知設定、更新するパスワード(ハッシュ化済み)、更新するステータスを利用してアカウントの情報を更新するためのメソッド。| 必要 |
| DeleteAccount | 空を送り、アカウントを削除するためのメソッド。| 必要 |

### Auth
    rpc Authenticate(V0AccountAuthenticateRequest) returns (V0ApiSessionData);
    rpc Unauthenticate(google.protobuf.Empty) returns (google.protobuf.Empty);
    rpc VerifyToken(google.protobuf.Empty) returns (V0ApiSessionData);

| RPC名  | 説明| 認証|
|--------|----|-----|
| Authenticate      | ユーザーのメールアドレスとユーザーのパスワード(ハッシュ化済み)を利用して、アカウントを認証認可するためのメソッド。| 不要 |
| Unauthenticate    | メタデータと一緒に空を送りセッションを終わらせるためのメソッド。| 必要 |
| VerifyToken | 空を送り、トークンを確認するためのメソッド。| 必要 |

## Organizations
### chats
    rpc ListChats (V0ListXRequest) returns (V0ListChatsResponse);
    rpc GetChat (V0GetOrDeleteXRequest) returns (Chat);
    rpc CreateChat (V0CreateChatRequest) returns (V0EventInvokedResponse);
    rpc UpdateChat (V0UpdateChatRequest) returns (V0EventInvokedResponse);
    rpc DeleteChat (V0GetOrDeleteXRequest) returns (V0EventInvokedResponse);

| RPC名  | 説明| 認証|
|--------|----|-----|
| ListChats      | 識別IDとページサイズとページ番号を利用してチャットのリストを取得するためのメソッド。| 必要 |
| GetChat    | 識別IDと組織IDを利用してチャットを取得するためのメソッド。| 必要 |
| CreateChat | 識別IDと組織IDとタイプを利用してチャット作成するためのメソッド。| 必要 |
| UpdateChat | 組織IDとチャットIDと新しい名前と新しい説明を利用してチャットを更新するためのメソッド。| 必要 |
| DeleteChat | 識別IDと組織IDを利用してチャットを削除するためのメソッド。| 必要 |

### Infos
    rpc List (google.protobuf.Empty) returns (V0ListOrganizationsResponse);
    rpc Get (V0BaseRequest) returns (Organization);
    rpc Create (V0CreateOrganizationRequest) returns (V0EventInvokedResponse);
    rpc UpdateOrganization (V0UpdateOrganizationRequest) returns (V0EventInvokedResponse);
    rpc Delete (V0BaseRequest) returns (V0EventInvokedResponse);

| RPC名  | 説明| 認証|
|--------|----|-----|
| List      | 空を送り、組織の情報のリストを取得するためのメソッド。| 必要 |
| Get    | 識別IDを利用して組織の情報を取得するためのメソッド。| 必要 |
| Create | 組織IDと説明を利用して組織を作成するためのメソッド。| 必要 |
| UpdateOrganization | 組織IDと組織名と新しい説明を利用して組織を更新するためのメソッド。| 必要 |
| Delete | 識別IDを利用して組織を削除するためのメソッド。| 必要 |

### Members
    rpc ListMembers (V0ListXRequest) returns (V0ListMembersResponse);
    rpc GetMember (V0GetOrDeleteXRequest) returns (Member);
    rpc JoinMember (V0JoinMemberRequest) returns (V0EventInvokedResponse);
    rpc LeaveMember (V0LeaveMemberRequest) returns (V0EventInvokedResponse);

| RPC名  | 説明| 認証|
|--------|----|-----|
| ListMembers      | 識別IDとページサイズとページ番号を利用してメンバーのリストを取得するためのメソッド。| 必要 |
| GetMember    | 組織IDとメンバーIDを利用してメンバーを取得するためのメソッド。| 必要 |
| JoinMember | 組織IDとユーザーIDと招待コード(プライベートの場合)を利用してメンバーに参加するためのメソッド。| 必要 |
| LeaveMember | 組織IDとユーザーIDを利用してメンバーを退出するためのメソッド。| 必要 |

### Roles
    rpc ListRoles (V0ListXRequest) returns (V0ListRolesResponse);
    rpc GetRole (V0GetOrDeleteXRequest) returns (Role);
    rpc CreateRole (V0CreateRoleRequest) returns (V0EventInvokedResponse);
    rpc UpdateRole (Role) returns (V0EventInvokedResponse);
    rpc DeleteRole (V0GetOrDeleteXRequest) returns (V0EventInvokedResponse);

| RPC名  | 説明| 認証|
|--------|----|-----|
| ListRoles      | 識別IDとページサイズとページ番号を利用してロールのリストを取得するためのメソッド。| 必要 |
| GetRole    | 識別IDと組織IDを利用してロールを取得するためのメソッド。| 必要 |
| CreateRole | 識別IDと組織IDを利用してロールを作成するためのメソッド。| 必要 |
| UpdateRole | ロールIDと組織IDと名前と説明と作成日と更新日を利用してロールを更新するためのメソッド。| 必要 |
| DeleteRole | 識別IDと組織IDを利用してロールを削除するためのメソッド。| 必要 |

### Messages
    rpc ListMessages (V0ListMessagesRequest) returns (V0ListMessagesResponse);
    rpc GetMessage (V0GetOrDeleteMessageRequest) returns (Message);
    rpc CreateMessage (Message) returns (V0EventInvokedResponse);
    rpc UpdateMessage (Message) returns (V0EventInvokedResponse);
    rpc DeleteMessage (V0GetOrDeleteMessageRequest) returns (V0EventInvokedResponse);

| RPC名  | 説明| 認証|
|--------|----|-----|
| ListMessages      | チャットIDとページサイズとページ番号を利用してメッセージのリストを取得するためのメソッド。| 必要 |
| GetMessage    | 組織IDとチャットIDとメッセージIDを利用してメッセージを取得するためのメソッド。| 必要 |
| CreateMessage | メッセージIDと組織IDとチャットIDとセンダーIDとコンテントと作成日と更新日を利用してメッセージを作成するためのメソッド。| 必要 |
| UpdateMessage | メッセージIDと組織IDとチャットIDとセンダーIDとコンテントと作成日と更新日を利用してメッセージを更新するためのメソッド。| 必要 |
| DeleteMessage | 組織IDとチャットIDとメッセージIDを利用してメッセージを削除するためのメソッド。| 必要 |

# 3. EventService
    rpc Listen (ListenRequest) returns (stream V0EventData);
    rpc UpdateListen (ListenRequest) returns (google.protobuf.Empty);
    rpc Unlisten (UnlistenRequest) returns (google.protobuf.Empty);

| RPC名  | 説明|
|--------|----|
| Listen      | ユーザーIDと利用しているセッションIDとイベントトピックと組織IDリスト(グローバル)と組織ID(アクティブ)とダイレクトメッセージのIDリストを利用してイベントを購読するためのメソッド。|
| UpdateListen    | ユーザーIDと利用しているセッションIDとイベントトピックと組織IDリスト(グローバル)と組織ID(アクティブ)とダイレクトメッセージのIDリストを利用して購読を更新するためのメソッド。|
| Unlisten | 購読を停止するユーザーIDと利用しているセッションIDを利用して購読を停止するためのメソッド。|