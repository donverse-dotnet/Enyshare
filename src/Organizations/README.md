# Pocco Organizations
このリポジトリは、「Pocco」の組織を管理するためのサービスを提供しています。

## 環境
- Linux
- Docker
- .NET 9.0 SDK

## 実行方法
1. イメージを作成
`docker build -f .\src\servers\ChatService\Dockerfile -t pocco-org-chatservice:latest --rm .`
`docker build -f .\src\servers\InfoService\Dockerfile -t pocco-org-infoservice:latest --rm .`
`docker build -f .\src\servers\MemberService\Dockerfile -t pocco-org-memberservice:latest --rm .`
`docker build -f .\src\servers\MessageService\Dockerfile -t pocco-org-messageservice:latest --rm .`
`docker build -f .\src\servers\RoleService\Dockerfile -t pocco-org-roleservice:latest --rm .`
2. 各サービスのsample.envを`.env`にコピーし、内部を編集
3. 各サービスを起動
`docker run --name pocco-org-chatservice --env-file .\src\servers\ChatService\.env -d -p 5033:5033 pocco-org-chatservice:latest`
`docker run --name pocco-org-infoservice --env-file .\src\servers\InfoService\.env -d -p 5065:5065 pocco-org-infoservice:latest`
`docker run --name pocco-org-memberservice --env-file .\src\servers\MemberService.env -d -p 5295:5295 pocco-org-memberservice:latest`
`docker run --name pocco-org-messageservice --env-file .\src\servers\MessageService\.env -d -p 5017:5017 pocco-org-messageservice:latest`
`docker run --name pocco-org-roleservice --env-file .\src\servers\RoleService\.env -d -p 5227:5227 pocco-org-roleservice:latest`
