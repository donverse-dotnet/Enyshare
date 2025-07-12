using Grpc.Core;
using Auth;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualBasic;
using MyGrpcApp.Service;

public class AuthServiceImpl : AuthServer.AuthServerBase
{
  // 仮のユーザーデータ
  private readonly Dictionary<string, string> users = new()
  {
    {"alice@example.com", "password123" },
    {"bob@example.com", "securepass" }
  };

  private const string SecretKey = "your_super_secret_key_12345";

  public override Task<AuthResponse> SignIn(SignInRequest request, ServerCallContext context) {
    if (!users.TryGetValue(request.Email, out var storePassword) || storePassword != request.Password) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid email or password"));
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(SecretKey);

    var tokenDescriptor = new SecurityTokenDescriptor {
      Subject = new ClaimsIdentity(new[]
      {
        new Claim(ClaimTypes.Email, request.Email),
        new Claim("role", "user")
      }),
      Expires = DateTime.UtcNow.AddHours(1),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var accessToken = tokenHandler.WriteToken(token);

    var response = new AuthResponse
    {
      AccessToken = accessToken,
      RefreshToken = "dummy_refresh_token",
      ExpiresIn = 3600
    };

    return Task.FromResult(response);
  }
}
