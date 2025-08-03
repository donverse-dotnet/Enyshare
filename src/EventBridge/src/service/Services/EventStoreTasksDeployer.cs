using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MySql.Data.MySqlClient;
using Pocco.Svc.EventBridge.Contexts;
using Pocco.Svc.EventBridge.Contexts.Models;
using Pocco.Svc.EventBridge.Protobufs.Services;

namespace Pocco.Svc.EventBridge.Services;

public class EventStoreTasksDeployer(
  [FromServices] EventLogContext eventLogContext,
  ILogger<EventStoreTasksDeployer> logger
) {
  private readonly EventLogContext _eventLogContext = eventLogContext;
  private readonly ILogger<EventStoreTasksDeployer> _logger = logger;

  public async Task<bool> SaveEventDataAsync(string eventId, V0EventData.PayloadOneofCase eventType, object eventData) {
    try {
      var eventLog = new EventLogModel {
        EventId = eventId,
        EventType = eventType switch {
          V0EventData.PayloadOneofCase.AccountCreatedEvent => EventType.OnAccountCreated,
          V0EventData.PayloadOneofCase.AccountUpdatedEvent => EventType.OnAccountUpdated,
          V0EventData.PayloadOneofCase.AccountModeratedEvent => EventType.OnAccountModerated,
          V0EventData.PayloadOneofCase.AccountDisabledEvent => EventType.OnAccountDisabled,
          V0EventData.PayloadOneofCase.MessageCreatedEvent => EventType.OnOrganizationMessageCreated,
          V0EventData.PayloadOneofCase.MessageUpdatedEvent => EventType.OnOrganizationMessageUpdated,
          V0EventData.PayloadOneofCase.MessageDeletedEvent => EventType.OnOrganizationMessageDeleted,
          V0EventData.PayloadOneofCase.MessageReactionAddedEvent => EventType.OnOrganizationMessageReactionAdded,
          V0EventData.PayloadOneofCase.MessageReactionRemovedEvent => EventType.OnOrganizationMessageReactionRemoved,
          _ => throw new ArgumentException("Unsupported event type")
        },
        EventData = eventData.ToString() ?? "{}",
        FiredAt = DateTime.UtcNow
      };

      await _eventLogContext.EventLogs.AddAsync(eventLog);
      await _eventLogContext.SaveChangesAsync();
      _logger.LogInformation("Event data saved successfully for EventId: {EventId}", eventId);
      return true;
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to save event data for EventId: {EventId}", eventId);
      return false;
    }
  }
}
