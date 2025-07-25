using System;
using System.Text;
using Grpc.Core;
using System.Collections.Generic;
using Pocco.Srv.Auth;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Bson.Serialization.Serializers;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Http.Features;
using BCrypt.Net;
using Microsoft.VisualBasic;


namespace Pocco.Srv.Auth.Services;

public class AuthenticationGrpcService : AuthService.AuthServiceBase
{
  private ClaimsPrincipal? VerifyToken(string token) {
    var key = Encoding.ASCII.GetBytes("your-very-secure-secret-key");
    var tokenHandler = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(key),
      ValidateIssuer = false,
      ValidateAudience = false,
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero
    };

    try {
      SecurityToken validatedToken;
      var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
      return principal;
    } catch {
      return null;
    }
  }
    public override async Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context) {
    SignInResponse response = new SignInResponse();

    // MongoDBに接続してUsersコレクション取得
    MongoClient client = new MongoClient("mongodb://localhost:27017");
    IMongoDatabase database = client.GetDatabase("MyAppDb");
    IMongoCollection<BsonDocument> userCollection = database.GetCollection<BsonDocument>("Users");

    // Emailでユーザー検索
    FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("Email", request.Email);
    var matchedUsers = await userCollection.Find(filter).ToListAsync();


    if (matchedUsers.Count == 1) {
      var userDoc = matchedUsers[0];
      string storedHash = userDoc["PasswordHash"].AsString;

      // パスワード照合（BCrypt）
      bool verified = BCrypt.Net.BCrypt.Verify(request.Password, storedHash);

      if (BCrypt.Net.BCrypt.Verify(request.Password, storedHash)) {
        // トークン生成（JWT）
        var key = Encoding.ASCII.GetBytes("your-very-secure-secret-key");
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor {
          Subject = new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Name, request.Email)
                    }),
          Expires = DateTime.UtcNow.AddHours(1),
          SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        response.Token = tokenHandler.WriteToken(token);
        response.Success = true;
        return response;
      }
    }

    // 認証失敗
    response.Token = "";
    response.Success = false;
    return response;
  }

  public override async Task<SignOutResponse> SignOut(SignOutRequest request, ServerCallContext context) {
    SignOutResponse response = new SignOutResponse();

    var principal = VerifyToken(request.Token);
    if (principal == null) {
      response.Success = false;
      response.Message = "";
      response.ErrorMessage = "無効なトークンです";
      return response;
    }

    // MongoDB接続
      MongoClient client = new MongoClient("mongodb://localhost:27017");
    IMongoDatabase database = client.GetDatabase("MyAppDB");
    IMongoCollection<BsonDocument> sessionCollection = database.GetCollection<BsonDocument>("Sessions");

    // session_idでセッション情報を検索
    var filter = Builders<BsonDocument>.Filter.Eq("SessionId", request.SessionId);
    var sessionDoc = await sessionCollection.Find(filter).FirstOrDefaultAsync();

    if (sessionDoc != null) {
      // セッションが存在すれば削除
      await sessionCollection.DeleteOneAsync(filter);

      response.Success = true;
      response.Message = "セッションが無効化されました";
    } else {
      response.Success = false;
      response.Message = "メッセージが見つかりません";
    }

    return response;
  }
}
