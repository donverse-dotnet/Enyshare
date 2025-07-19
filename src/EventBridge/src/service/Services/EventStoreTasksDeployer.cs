using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Pocco.Svc.EventBridge.Protobufs.Services;

namespace Pocco.Svc.EventBridge.Services;

public class EventStoreTasksDeployer(
  [FromServices] MongoClient mongoClient,
  ILogger<EventStoreTasksDeployer> logger
) {
  private readonly MongoClient _mongoClient = mongoClient;
  private readonly ILogger<EventStoreTasksDeployer> _logger = logger;

  public async Task<bool> SaveEventDataAsync(string eventId, V0EventData.PayloadOneofCase eventType, object eventData) {
    var database = _mongoClient.GetDatabase("EventStore");
    var collection = database.GetCollection<BsonDocument>("Events");

    try {
      // Create a sample event document
      var eventDocument = new BsonDocument {
        { "EventId", eventId },
        { "EventType", eventType.ToString() },
        { "EventData", BsonValue.Create(eventData) },
        { "Timestamp", DateTime.UtcNow }
      };

      // Insert the event document into the collection
      await collection.InsertOneAsync(eventDocument);
      _logger.LogInformation("Event data saved successfully.");
      return true;
    } catch (Exception ex) {
      _logger.LogError(ex, "Failed to save event data.");
      return false;
    }
  }
}
