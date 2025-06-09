<img src="./docs/assets/pocco-cdn-bunner.png" height="360" width="640">

<hr />

# Pocco.CDN

このプロジェクトは、 @ggg-alpha が作成する`Pocco`のテスト用CDNサーバーを提供します。

## 動作環境
- Windows/Linux
- .NET 9.0 SDK

## 注意事項
1. このサービスは、自動で全世界にデプロイされるわけではありません
2. このサービスを本番環境で利用するには、`GoogleCloud Cloud DNS`などを利用して地理的に近い場所のVMにルーティングする必要があります
3. 多くの場合、`Cloudflare CDN`や`GoogleCloud Cloud CDN`、`AWS Cloudfront`などを利用する方が効率的で理想的です
