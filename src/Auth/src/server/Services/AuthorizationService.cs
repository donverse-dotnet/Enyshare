using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using MongoDB.Driver;

using Pocco.Libs.Protobufs.Auth.Types;
using Pocco.Srv.Auth.Models;


namespace Pocco.Srv.Auth.Services;

public partial class V0AuthServiceImpl {
  public override async Task<V0SessionData> Auth(V0SessionData request, ServerCallContext context) {
    var principal = _jwtTokenHandler.VerifyToken(request.Token);

    if (principal is null) {
      _logger.LogWarning("Invalid token provided.");
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token."));
    }

    var newToken = _jwtTokenHandler.RegenerateToken(request.Token, DateTime.UtcNow.AddHours(1));

    if (string.Equals(newToken, request.Token) is true) {
      _logger.LogInformation("Token has not changed, skipping database update. SessionId: {SessionId}", request.SessionId);
      return request;
    }

    // セッション情報をMongoDBに保存
    var newSessionData = new V0SessionDataWrapper {
      SessionId = request.SessionId,
      AccountId = request.AccountId,
      Token = newToken,
      CreatedAt = request.CreatedAt,
      ExpiresAt = Timestamp.FromDateTime(DateTime.UtcNow.AddHours(1)),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
    };

    var dbUpdateResult = await _sessionsCollection.ReplaceOneAsync(
      x => x.SessionId == request.SessionId,
      newSessionData,
      new ReplaceOptions { IsUpsert = true }
    );

    if (dbUpdateResult.IsAcknowledged && dbUpdateResult.ModifiedCount == 0) {
      _logger.LogWarning("Session data not updated for SessionId: {SessionId}", request.SessionId);
    } else {
      _logger.LogInformation("Session data updated successfully for SessionId: {SessionId}", request.SessionId);
    }

    return newSessionData.ToV0SessionData();
  }
}
