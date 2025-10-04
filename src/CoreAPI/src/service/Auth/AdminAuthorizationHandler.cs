using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pocco.Svc.CoreAPI.Auth;

public class AdminAuthorizationHandler : AuthorizationHandler<AuthorizationRequirement> {
  private readonly ILogger<AdminAuthorizationHandler> _logger;

  public AdminAuthorizationHandler(
    [FromServices] ILogger<AdminAuthorizationHandler> logger
  ) {
    _logger = logger;
    _logger.LogInformation("AdminHandler initialized.");
  }

  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement) {
    if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin")) {
      context.Succeed(requirement);
    } else {
      _logger.LogWarning("Authorization failed: User does not have the required role.");
      context.Fail();
    }
    return Task.CompletedTask;
  }
}
