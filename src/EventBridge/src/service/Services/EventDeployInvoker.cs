using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Services.Handlers;

namespace Pocco.Svc.EventBridge.Services;

public class EventDeployInvoker(
  [FromServices] EventDispatcher eventSender,
  ILogger<EventDeployInvoker> logger
) {
  private readonly ILogger<EventDeployInvoker> _logger = logger;
  private readonly EventDispatcher _eventSender = eventSender;

  public async Task<bool> AddEventToQueueAsync(
    string eventId,
    V0EventData eventData
  ) {
    _logger.LogInformation("Adding event to queue: {EventId}", eventId);
    bool result = await _eventSender.AddEventToQueueAsync(eventId, eventData);
    if (result) {
      _logger.LogInformation("Event added to queue successfully: {EventId}", eventId);
    } else {
      _logger.LogError("Failed to add event to queue: {EventId}", eventId);
    }
    return result;
  }
}
