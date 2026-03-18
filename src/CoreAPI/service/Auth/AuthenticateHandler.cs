using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Pocco.Libs.Protobufs.Auth.Services;
using Pocco.Libs.Protobufs.Auth.Types;

namespace Pocco.Svc.CoreAPI.Auth;

public class AuthenticateHandler(
  IOptionsMonitor<AuthenticationSchemeOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder,
  V0AuthService.V0AuthServiceClient v0AuthServiceClient
) : AuthenticationHandler<AuthenticationSchemeOptions>(
  options,
  logger,
  encoder
) {
  private readonly V0AuthService.V0AuthServiceClient _v0AuthServiceClient = v0AuthServiceClient;

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
    Logger.LogInformation("Handling authentication for request: {RequestPath} (from {ip})", Request.Path, Request.HttpContext.Connection.RemoteIpAddress);

    // Get data from headers(metadata)
    var tokenFound = Request.Headers.TryGetValue("authorization", out var token);
    var sessionIdFound = Request.Headers.TryGetValue("x-session-id", out var sessionId);
    var accountIdFound = Request.Headers.TryGetValue("x-account-id", out var accountId);
    var createdAtFound = Request.Headers.TryGetValue("x-created-at", out var createdAt);
    var expiresAtFound = Request.Headers.TryGetValue("x-expires-at", out var expiresAt);
    var updatedAtFound = Request.Headers.TryGetValue("x-updated-at", out var updatedAt);

    if (!tokenFound || !sessionIdFound || !accountIdFound || !expiresAtFound || !createdAtFound || !updatedAtFound) {
      Logger.LogWarning("Missing required headers: Token: {TokenFound}, SessionId: {SessionIdFound}, AccountId: {AccountIdFound}, ExpiresAt: {ExpiresAtFound}, CreatedAt: {CreatedAtFound}, UpdatedAt: {UpdatedAtFound}",
        tokenFound, sessionIdFound, accountIdFound, expiresAtFound, createdAtFound, updatedAtFound);
      return AuthenticateResult.Fail("Required headers are missing.");
    }

    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(expiresAt) || string.IsNullOrEmpty(createdAt) || string.IsNullOrEmpty(updatedAt)) {
      Logger.LogWarning("Session data is incomplete: SessionId: {SessionId}, AccountId: {AccountId}, ExpiresAt: {ExpiresAt}, CreatedAt: {CreatedAt}, UpdatedAt: {UpdatedAt}",
        sessionId, accountId, expiresAt, createdAt, updatedAt);
      return AuthenticateResult.Fail("Session data is incomplete.");
    }

    if (!token.ToString().StartsWith("Bearer ")) {
      Logger.LogWarning("Invalid or empty token.");
      return AuthenticateResult.Fail("Invalid or empty token.");
    }

    var accessToken = token.ToString().Substring("Bearer ".Length).Trim();
    if (string.IsNullOrEmpty(accessToken)) {
      return AuthenticateResult.Fail("Access token is empty.");
    }

    Logger.LogInformation("Loaded auth data from headers for session ID: {SessionId}, account ID: {AccountId}, expires at: {ExpiresAt}, created at: {CreatedAt}, updated at: {UpdatedAt}",
      sessionId, accountId, expiresAt, createdAt, updatedAt);

    // Verify token
    try {
      Logger.LogInformation("Verifying access token: {AccessToken} for {SessionId}", accessToken, sessionId);

      var expiresAtData = JsonSerializer.Deserialize<Timestamp>(expiresAt.ToString());
      var createdAtData = JsonSerializer.Deserialize<Timestamp>(createdAt.ToString());
      var updatedAtData = JsonSerializer.Deserialize<Timestamp>(updatedAt.ToString());

      var res = await _v0AuthServiceClient.AuthAsync(new V0SessionData {
        SessionId = sessionId.ToString(),
        AccountId = accountId.ToString(),
        Token = accessToken,
        ExpiresAt = expiresAtData,
        CreatedAt = createdAtData,
        UpdatedAt = updatedAtData
      });

      if (res is null) {
        Logger.LogWarning("Token verification failed for session ID: {SessionId}", sessionId.ToString());
        return AuthenticateResult.Fail("Token verification failed.");
      }

      // Read res.Token and get role
      var handler = new JwtSecurityTokenHandler();
      var tokenObj = handler.ReadJwtToken(res.Token);
      var roleClaim = tokenObj.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role) ?? tokenObj.Claims.FirstOrDefault(c => c.Type == "role");
      Logger.LogInformation("Extracted role claim: {RoleClaim} from token for session ID: {SessionId}", roleClaim?.Value, sessionId.ToString());
      if (roleClaim is null) {
        Logger.LogWarning("Role claim not found in token for session ID: {SessionId}", sessionId.ToString());
        return AuthenticateResult.Fail("Role claim not found in token.");
      }

      // Create claims from the session data
      var claims = new List<Claim> {
        new(ClaimTypes.Actor, res.SessionId),
        new(ClaimTypes.NameIdentifier, res.AccountId),
        new(ClaimTypes.Role, roleClaim.Value)
      };
      var identity = new ClaimsIdentity(claims, Scheme.Name);
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, Scheme.Name);

      Logger.LogInformation("Token verified successfully for session ID: {SessionId}", sessionId.ToString());

      // Return the authentication result
      return AuthenticateResult.Success(ticket);
    } catch (RpcException ex) {
      if (ex.StatusCode == StatusCode.Unauthenticated) {
        Logger.LogWarning("Authentication failed for access token: {AccessToken} with status code: {StatusCode}", accessToken, ex.StatusCode);
        return AuthenticateResult.Fail("Invalid access token.");
      }
      Logger.LogError(ex, "An error occurred while verifying the access token: {AccessToken}", accessToken);
      return AuthenticateResult.Fail($"Something went wrong while verifying the token.");
    }
  }
}
