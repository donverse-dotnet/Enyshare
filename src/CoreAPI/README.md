# Pocco CoreAPI
このリポジトリは、「Pocco」のUIRendererと内部サービスとのデータのやり取りを提供するサービスです。

## 環境
- Linux
- Docker

## 実行方法
1. `.env.sample`を`.env`にコピーして、値を編集する
2. `docker build -t pocco-coreapi:latest --rm .`
3. `docker run --name pocco-coreapi --env-file .env -d -p 5280:5280 pocco-coreapi:latest`
