# Pocco Accounts
このリポジトリは、「Pocco」のアカウント管理機能を提供するサービスです。

## 環境
- Linux
- Docker

## 実行方法
1. `.env.sample`を`.env`にコピーして、値を編集する
2. `docker build -t pocco-accounts:latest --rm .`
3. `docker run --name pocco-accounts --env-file .env -d -p 5083:5083 pocco-accounts:latest`
