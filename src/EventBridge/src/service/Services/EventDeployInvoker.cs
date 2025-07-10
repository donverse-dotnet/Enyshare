using Microsoft.AspNetCore.Mvc;

namespace Pocco.Svc.EventBridge.Services;

public class EventDeployInvoker(
  [FromServices] EventSender eventSender,
  ILogger<EventDeployInvoker> logger
) {
  private readonly ILogger<EventDeployInvoker> _logger = logger;
  private readonly EventSender _eventSender = eventSender;
}
