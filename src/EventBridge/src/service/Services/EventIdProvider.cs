using Microsoft.AspNetCore.Mvc;

namespace Pocco.Svc.EventBridge.Services;

public class EventIdProvider : IHotStartableService {
  public EventIdProvider([FromServices] ILogger<EventIdProvider> logger) {
    _logger = logger;
    _logger.LogInformation("EventIdProvider initialized");
  }

  private readonly ILogger<EventIdProvider> _logger;

  public Task WarmUpAsync(IServiceProvider sp, CancellationToken cancellationToken) {
    _logger.LogInformation("EventIdProvider warming up...");
    _logger.LogInformation("EventIdProvider warmed up.");
    return Task.CompletedTask;
  }

  public Task CoolDownAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("EventIdProvider cooling down...");
    _logger.LogInformation("EventIdProvider cooled down.");
    return Task.CompletedTask;
  }

  public string GenerateEventId() {
    // UNIXTIME_RANDOM8DIGITS_GUID
    var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    var random = Random.Shared.Next(10000000, 99999999);
    var guid = Guid.NewGuid().ToString("N");
    var eventId = $"{unixTime}_{random}_{guid}";

    _logger.LogDebug("Generated Event ID: {EventId}", eventId);
    return eventId;
  }
}
