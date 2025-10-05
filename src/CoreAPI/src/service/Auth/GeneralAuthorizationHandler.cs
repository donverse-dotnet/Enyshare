using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pocco.Svc.CoreAPI.Auth;

public class GeneralAuthorizationHandler : AuthorizationHandler<AuthorizationRequirement> {
  private readonly ILogger<GeneralAuthorizationHandler> _logger;

  public GeneralAuthorizationHandler(
    [FromServices] ILogger<GeneralAuthorizationHandler> logger
  ) {
    _logger = logger;
    _logger.LogInformation("GeneralHandler initialized.");
  }

  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement) {
    if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "User" || c.Value == "Admin"))) {
      _logger.LogInformation("Authorization succeeded: {UserId} has the required role.", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
      context.Succeed(requirement);
    } else {
      _logger.LogWarning("Authorization failed: User does not have the required role.");
      // context.Fail(); // いずれかのハンドラが失敗を発行した場合、認可が失敗するため、明示的に失敗を設定する必要はありません
    }
    return Task.CompletedTask;
  }
}
