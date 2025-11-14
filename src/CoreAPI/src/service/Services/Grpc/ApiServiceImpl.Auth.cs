using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Libs.Protobufs.Auth.Services;
using Pocco.Libs.Protobufs.Auth.Types;
using Pocco.Libs.Protobufs.Services;

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
  public override async Task<Empty> Unauthenticate(Empty request, ServerCallContext context) {
    var urd = new V0SessionData {
      SessionId = "" // TODO: コンテキストからセッションIDを取得
    };

    var response = await _authServiceClient.SignOutAsync(urd, cancellationToken: context.CancellationToken);

    return new Empty();
  }
  // VerifyToken
  public override async Task<V0ApiSessionData> VerifyToken(Empty request, ServerCallContext context) {
    var vtrd = new V0SessionData {
      SessionId = "" // TODO: コンテキストからセッションIDを取得
    };

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
}
