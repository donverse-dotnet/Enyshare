using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

using Pocco.Svc.EventBridge.Utilities;

namespace Pocco.Svc.EventBridge.Services;

public class EventSender(
  [FromServices] MongoClient mongoClient,
  [FromServices] ILogger<EventSender> logger
) {
  public readonly Dictionary<string, IServerStreamWriter<SubscribeEventStreamData>> ClientList = [];
  public readonly Dictionary<string, EventData> EventSendQueue = [];

  private readonly MongoClient _mongoClient = mongoClient;
  private readonly ILogger<EventSender> _logger = logger;

  public class EventData(string eventId, DeployEventRequest.EventDataOneofCase eventType, object data) {
    public readonly string EventId = eventId;
    public readonly DeployEventRequest.EventDataOneofCase EventType = eventType;
    public readonly object Data = data;
  }

  public async Task AddClientAsync(
    string accountId,
    IServerStreamWriter<SubscribeEventStreamData> responseStream
  ) {
    if (!ClientList.ContainsKey(accountId)) {
      ClientList[accountId] = responseStream;
      await Task.CompletedTask;
    }
  }

  public async Task RemoveClientAsync(string accountId) {
    if (ClientList.ContainsKey(accountId) is true) {
      ClientList.Remove(accountId);
      await Task.CompletedTask;
    }
  }

  public async Task<bool> AddEventToQueueAsync(
    string eventId,
    DeployEventRequest eventData
  ) {
    if (EventSendQueue.ContainsKey(eventId) is false) {
      EventSendQueue[eventId] = new EventData(
        eventId,
        eventData.EventDataCase,
        data: GrpcServiceHelper.GetEventData(eventData)
      );
    } else {
      // 既に同じイベントIDがキューに存在する場合は、何もしないか、上書きするかの処理を行う
      // ここでは上書きする例を示す
      EventSendQueue[eventId] = new EventData(
        eventId,
        eventData.EventDataCase,
        data: GrpcServiceHelper.GetEventData(eventData)
      );
    }
    await Task.CompletedTask;
    return true;
  }

  // TODO: クライアントにイベントを送信するメソッドを実装する
}
