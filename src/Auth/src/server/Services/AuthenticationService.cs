using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Grpc.Core;

using Microsoft.IdentityModel.Tokens;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;


namespace Pocco.Srv.Auth.Services;

public class AuthenticationGrpcService : AuthService.AuthServiceBase {
  private class AccountsModel {
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("Email")]
    public string? Email { get; set; }

    [BsonElement("PasswordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }
  }

  private readonly IMongoCollection<AccountsModel> _userCollection;

  private List<string?> emails = new();

  public AuthenticationGrpcService(IMongoDatabase database) {
    _userCollection = database.GetCollection<AccountsModel>("Users");
  }
  public async Task InitializeAsync(SignInRequest req) {
    emails = await _userCollection
    .Find(x => x.Email == req.Email)
    .Project(x => x.Email)
    .ToListAsync();
  }

  private static MongoClient dbClient = new MongoClient("mongodb://localhost:27017");
  private static IMongoDatabase database = dbClient.GetDatabase("MyAppDb");

  private IMongoCollection<AccountsModel> userCollection = database.GetCollection<AccountsModel>("Users");
  private IMongoCollection<BsonDocument> sessionCollection = database.GetCollection<BsonDocument>("Sessions");
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

    // Emailでユーザー検索
    FilterDefinition<AccountsModel> filter = Builders<AccountsModel>.Filter.Eq(x => x.Email, request.Email);
    var matchedUsers = await userCollection.Find(filter).ToListAsync();


    if (matchedUsers.Count == 1) {
      var userDoc = matchedUsers[0];
      string? storedHash = userDoc.PasswordHash;

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

    // session_idでセッション情報を検索
    var filter = Builders<BsonDocument>.Filter.Eq("SessionId", request.SessionId);
    var sessionDoc = await sessionCollection.Find(filter).FirstOrDefaultAsync();

    string sessionAccountId = sessionDoc["AccountId"].AsString;

    if (sessionAccountId != request.AccountId) {
      response.Success = false;
      response.Message = "アカウントIDが一致しません";
      return response;
    }

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
