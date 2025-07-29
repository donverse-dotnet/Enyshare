using System.Security.Claims;

namespace Pocco.Srv.Auth.Interfaces;

public interface IJwtTokenHandler {
  // Generate token
  string GenerateToken(ClaimsPrincipal principal, DateTime expiration);
  // Verify token
  ClaimsPrincipal? VerifyToken(string token);
  // Regenerate token
  string RegenerateToken(string token, DateTime newExpiration);
}
