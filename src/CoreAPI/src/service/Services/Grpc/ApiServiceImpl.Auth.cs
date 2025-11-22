using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Pocco.Libs.Protobufs.Auth.Services;
using Pocco.Libs.Protobufs.Auth.Types;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public partial class ApiServiceImpl {
  private readonly V0AuthService.V0AuthServiceClient _authServiceClient;

  // Authenticate
  public override async Task<V0ApiSessionData> Authenticate(V0AccountAuthenticateRequest request, ServerCallContext context) {
    var ard = new V0SignInRequest {
      Email = request.Email,
      Password = request.Password
    };

    var response = await _authServiceClient.SignInAsync(ard, cancellationToken: context.CancellationToken);

    var sessionData = new V0ApiSessionData {
      SessionId = response.SessionId,
      AccountId = response.AccountId,
      Token = response.Token,
      ExpiresAt = response.ExpiresAt,
      CreatedAt = response.CreatedAt,
      UpdatedAt = response.UpdatedAt
    };

    return sessionData;
  }

  // Unauthenticate
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<Empty> Unauthenticate(Empty request, ServerCallContext context) {
    var urd = BuildSessionDataFromContext(context);

    var response = await _authServiceClient.SignOutAsync(urd, cancellationToken: context.CancellationToken);

    return new Empty();
  }

  // VerifyToken
  [Authorize(Policy = "RequireGeneral")]
  public override async Task<V0ApiSessionData> VerifyToken(Empty request, ServerCallContext context) {
    _logger.LogInformation("Verifying token for request => {header}", context.RequestHeaders);
    var vtrd = BuildSessionDataFromContext(context);

    var response = await _authServiceClient.AuthAsync(vtrd, cancellationToken: context.CancellationToken);

    var sessionData = new V0ApiSessionData {
      SessionId = response.SessionId,
      AccountId = response.AccountId,
      Token = response.Token,
      ExpiresAt = response.ExpiresAt,
      CreatedAt = response.CreatedAt,
      UpdatedAt = response.UpdatedAt
    };

    return sessionData;
  }


  private V0SessionData BuildSessionDataFromContext(ServerCallContext context) {
    var headers = GetSessionFromContext(context);
    var sessionData = new V0SessionData();

    foreach (var header in headers) {
      _logger.LogInformation("Processing header: {Key} => {Value}", header.Key, header.Value);

      switch (header.Key) {
        case "authorization":
          if (header.Value.StartsWith("Bearer ")) {
            sessionData.Token = header.Value.Substring("Bearer ".Length).Trim();
          }
          break;
        case "x-session-id":
          sessionData.SessionId = header.Value;
          break;
        case "x-account-id":
          sessionData.AccountId = header.Value;
          break;
        case "x-created-at":
          sessionData.CreatedAt = JsonSerializer.Deserialize<Timestamp>(header.Value);
          break;
        case "x-expires-at":
          sessionData.ExpiresAt = JsonSerializer.Deserialize<Timestamp>(header.Value);
          break;
        case "x-updated-at":
          sessionData.UpdatedAt = JsonSerializer.Deserialize<Timestamp>(header.Value);
          break;
        default:
          _logger.LogWarning("Unknown header key: {Key}", header.Key);
          break;
      }
    }

    return sessionData;
  }

  private Dictionary<string, string> GetSessionFromContext(ServerCallContext context) {
    var headers = new Dictionary<string, string>();

    foreach (var header in context.RequestHeaders) {
      headers[header.Key] = header.Value;
    }

    return headers;
  }
}
