using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

using Grpc.Core;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using Pocco.Srv.Auth;

namespace Pocco.Srv.Auth.Services;

public class AuthorizationService : AuthService.AuthServiceBase {
  private readonly TokenValidationParameters _tokenValidationParameters;

  public AuthorizationService(TokenValidationParameters tokenValidationParameters) {
    _tokenValidationParameters = tokenValidationParameters;
  }

  public ClaimsPrincipal? ValidateToken(string token) {
    var tokenHandler = new JwtSecurityTokenHandler();

    try {
      var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters,
      out SecurityToken validatedToken);

      if (validatedToken is JwtSecurityToken jwtToken &&
      jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
      {
        return principal;
      }
    } catch (Exception ex) {
      Console.WriteLine($"Token validation failed: {ex.Message}");
    }
    return null;
  }

  public bool IsAuthorized(ClaimsPrincipal principal, string requiredRole) {
    if (principal == null || !(principal.Identity?.IsAuthenticated ?? false))
      return false;

    return principal.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == requiredRole);
  }
}
