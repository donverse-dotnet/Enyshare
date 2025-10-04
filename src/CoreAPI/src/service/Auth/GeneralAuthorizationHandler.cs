using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pocco.Svc.CoreAPI.Auth;

public class GeneralAuthorizationHandler : AuthorizationHandler<AuthorizationRequirement>, IAuthorizationRequirement {
  private readonly ILogger<GeneralAuthorizationHandler> _logger;

  public GeneralAuthorizationHandler(
    [FromServices] ILogger<GeneralAuthorizationHandler> logger
  ) {
    _logger = logger;
    _logger.LogInformation("GeneralHandler initialized.");
  }

  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement) {
    if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && (c.Value == "General" || c.Value == "Admin"))) {
      context.Succeed(requirement);
    } else {
      _logger.LogWarning("Authorization failed: User does not have the required role.");
      context.Fail();
    }
    return Task.CompletedTask;
  }
}
