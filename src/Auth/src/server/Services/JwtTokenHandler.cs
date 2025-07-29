using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

using Pocco.Srv.Auth.Interfaces;


namespace Pocco.Srv.Auth.Services;

public class JwtTokenHandler(ILogger<JwtTokenHandler> logger) : IJwtTokenHandler {
  private readonly ILogger<JwtTokenHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  private readonly string _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                  throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");

  public string GenerateToken(ClaimsPrincipal principal, DateTime expiration) {
    ArgumentNullException.ThrowIfNull(principal);

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var tokenDescriptor = new SecurityTokenDescriptor {
      Subject = principal.Identity as ClaimsIdentity,
      Expires = expiration,
      SigningCredentials = creds
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  public string RegenerateToken(string token, DateTime newExpiration) {
    if (string.IsNullOrWhiteSpace(token)) {
      throw new ArgumentException("Token cannot be null or empty.", nameof(token));
    }

    var principal = VerifyToken(token) ?? throw new InvalidOperationException("Invalid token.");
    return GenerateToken(principal, newExpiration);
  }

  public ClaimsPrincipal? VerifyToken(string token) {
    if (string.IsNullOrWhiteSpace(token)) {
      _logger.LogWarning("Token is null or empty.");
      return null;
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    try {
      var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken validatedToken);

      return principal;
    } catch (Exception ex) {
      _logger.LogError(ex, "Token verification failed.");
      return null;
    }
  }
}
