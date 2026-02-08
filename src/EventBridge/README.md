# Pocco EventBridge
このリポジトリは、「Pocco」のイベントをCoreAPIを介してユーザーに届けるサービスです。

## 環境
- Linux
- Docker

## 実行方法
1. `.env.sample`を`.env`にコピーして、値を編集する
2. `docker build -t pocco-eventbridge:latest --rm .`
3. `docker run --name pocco-eventbridge --env-file .env -d -p 5218:5218 pocco-eventbridge:latest`
