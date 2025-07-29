using System.Security.Claims;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using Pocco.Libs.Protobufs.Services;
using Pocco.Libs.Protobufs.Types;


namespace Pocco.Srv.Auth.Services;

public partial class V0AuthServiceImpl(
  [FromServices] JwtTokenHandler jwtTokenHandler,
  ILogger<V0AuthServiceImpl> logger
) : V0AuthService.V0AuthServiceBase {

  // TODO: ユーザーモデルをアカウントサービスから持ってくる
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

  private readonly ILogger<V0AuthServiceImpl> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  private readonly JwtTokenHandler _jwtTokenHandler = jwtTokenHandler ?? throw new ArgumentNullException(nameof(jwtTokenHandler));

  private static readonly string _connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ??
                  throw new InvalidOperationException("MONGO_CONNECTION_STRING environment variable is not set.");
  private static readonly MongoClient _dbClient = new(_connectionString);
  private static readonly IMongoDatabase _database = _dbClient.GetDatabase("Entities");
  private readonly IMongoCollection<AccountsModel> _usersCollection = _database.GetCollection<AccountsModel>("Accounts");
  private readonly IMongoCollection<V0SessionData> _sessionsCollection = _database.GetCollection<V0SessionData>("Sessions");

  public override async Task<V0SessionData> SignIn(V0SignInRequest request, ServerCallContext context) {
    // Emailでユーザー検索
    var user = await _usersCollection.Find(x => x.Email == request.Email && x.PasswordHash == request.Password).FirstOrDefaultAsync();

    if (user is null) {
      _logger.LogWarning("User not found for email: {Email}", request.Email);
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid email or password."));
    }

    // ユーザーが見つかった場合、トークンを作成
    var claims = new List<Claim> {
      new(ClaimTypes.Name, request.Email),
      new("UserId", user.Id.ToString()) // ユーザーモデルをアカウントサービスから持ってくる
    };
    string token = _jwtTokenHandler.GenerateToken(new ClaimsPrincipal(new ClaimsIdentity(claims)), DateTime.UtcNow.AddHours(1));
    var sessionId = ObjectId.GenerateNewId().ToString();

    var sessionData = new V0SessionData {
      SessionId = sessionId,
      AccountId = user.Id.ToString(),
      Token = token,
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      ExpiresAt = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(1)),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    };

    // セッション情報をMongoDBに保存
    await _sessionsCollection.InsertOneAsync(sessionData);

    _logger.LogInformation("User signed in: {Email}", request.Email);
    return sessionData;
  }

  public override async Task<V0SignOutResponse> SignOut(V0SessionData request, ServerCallContext context) {
    var principal = _jwtTokenHandler.VerifyToken(request.Token);

    if (principal is null) {
      return new V0SignOutResponse {
        Success = false,
        ErrorMessage = "無効なトークンです。強制的にサインアウトします。",
      };
    }

    // session_idでセッション情報を検索
    var filter = Builders<BsonDocument>.Filter.Eq("SessionId", request.SessionId);
    var sessionData = await _sessionsCollection.Find(x => x.AccountId == request.AccountId && x.SessionId == request.SessionId).FirstOrDefaultAsync();

    if (sessionData is null) {
      return new V0SignOutResponse {
        Success = false,
        ErrorMessage = "セッションが見つかりません。強制的にサインアウトします。"
      };
    }

    var dbDeleteResult = await _sessionsCollection.DeleteOneAsync(x => x.AccountId == request.AccountId && x.SessionId == request.SessionId);

    if (dbDeleteResult.DeletedCount == 0) {
      return new V0SignOutResponse {
        Success = false,
        ErrorMessage = "セッションの削除に失敗しました。強制的にサインアウトします。"
      };
    }

    _logger.LogInformation("User signed out: {Email}", principal.Identity?.Name);
    var response = new V0SignOutResponse {
      Success = true,
      ErrorMessage = string.Empty
    };

    return response;
  }
}
