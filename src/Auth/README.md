# Pocco Implements Template

## 使い方
- 実装部分は`src`ディレクトリ配下で行ってください
- このリポジトリで作成した機能を`docs`ディレクトリ配下にドキュメント化してください

## プロジェクトの作り方
1. 「Use this template」ボタンをクリック
2. 新しいリポジトリの情報を入力
3. お使いのGitHub（Git）クライアントでクローンして開発を開始しましょう！

--- 切り取り - ここから上を削除してリポジトリの説明を追加してください - 切り取り ---

# Pocco Auth
このリポジトリは、「Pocco」の認証を提供するサービスです。

## 環境
- Linux
- Docker

## 実行方法
1. `.env.sample`を`.env`にコピーして、値を編集する
2. `docker build -t pocco-auth:latest --rm .`
3. `docker run --name pocco-auth --env-file .env -d -p 5290:5290 pocco-auth:latest`
