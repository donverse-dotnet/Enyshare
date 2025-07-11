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
      // 上書きする
      EventSendQueue[eventId] = new EventData(
        eventId,
        eventData.EventDataCase,
        data: GrpcServiceHelper.GetEventData(eventData)
      );
    }
    await Task.CompletedTask;
    return true;
  }

  public List<IServerStreamWriter<SubscribeEventStreamData>> SelectTargetClients(DeployEventRequest.EventDataOneofCase eventType, string id) {
    var eventCategory = GrpcServiceHelper.GetEventCategory(eventType);

    var targetClientIds = eventCategory switch {
      GrpcServiceHelper.EventCategory.Account => _mongoClient.GetDatabase("pocco")
                                                             .GetCollection<FakeAccount>("accounts")
                                                             .Find(account => account.ListenUserEvents.Contains(id))
                                                             .Project(account => account.Id)
                                                             .ToList(),
      GrpcServiceHelper.EventCategory.Organization => _mongoClient.GetDatabase("pocco")
                                                                  .GetCollection<FakeAccount>("accounts")
                                                                  .Find(account => account.ListenOrganizationEvents.Contains(id))
                                                                  .Project(account => account.Id)
                                                                  .ToList(),
      GrpcServiceHelper.EventCategory.Message => _mongoClient.GetDatabase("pocco")
                                                             .GetCollection<FakeAccount>("accounts")
                                                             .Find(account => account.ListenMessageEvents.Contains(id))
                                                             .Project(account => account.Id)
                                                             .ToList(),
      _ => throw new ArgumentException("Unsupported event type", nameof(eventType))
    };

    return ClientList
      .Where(client => targetClientIds.Contains(client.Key))
      .Select(client => client.Value)
      .ToList();
  }

  public async Task SendToAffectedClientsAsync(List<IServerStreamWriter<SubscribeEventStreamData>> clients, string eventId, DeployEventRequest eventData) {
    foreach (var client in clients) {
      try {
        await client.WriteAsync(new SubscribeEventStreamData {
          EventId = eventId,
          AuthorId = eventData.AccountId,
          // TODO: イベントデータは複数代入してはいけないので、処理を分散させる
          AccountCreationRequestedEvent = eventData.AccountCreationRequestedEvent,
          AccountCreatedEvent = eventData.AccountCreatedEvent,
          AccountCreationFailedEvent = eventData.AccountCreationFailedEvent,
          AccountUpdatedRequestedEvent = eventData.AccountUpdatedRequestedEvent,
          AccountUpdatedEvent = eventData.AccountUpdatedEvent,
          AccountUpdateFailedEvent = eventData.AccountUpdateFailedEvent,
          AccountDeletionRequestedEvent = eventData.AccountDeletionRequestedEvent,
          AccountDeletedEvent = eventData.AccountDeletedEvent,
          AccountDeletionFailedEvent = eventData.AccountDeletionFailedEvent,
          OrganizationCreatedEvent = eventData.OrganizationCreatedEvent,
          OrganizationCreationFailedEvent = eventData.OrganizationCreationFailedEvent,
          OrganizationUpdatedRequestedEvent = eventData.OrganizationUpdatedRequestedEvent,
          OrganizationUpdatedEvent = eventData.OrganizationUpdatedEvent,
          OrganizationUpdateFailedEvent = eventData.OrganizationUpdateFailedEvent,
          OrganizationDeletionRequestedEvent = eventData.OrganizationDeletionRequestedEvent,
          OrganizationDeletedEvent = eventData.OrganizationDeletedEvent,
          OrganizationDeletionFailedEvent = eventData.OrganizationDeletionFailedEvent,
          MessageCreatedEvent = eventData.MessageCreatedEvent,
          MessageUpdatedEvent = eventData.MessageUpdatedEvent,
          MessageDeletedEvent = eventData.MessageDeletedEvent,
          OrganizationChannelCreationRequestedEvent = eventData.OrganizationChannelCreationRequestedEvent,
          OrganizationChannelCreatedEvent = eventData.OrganizationChannelCreatedEvent,
          OrganizationChannelCreationFailedEvent = eventData.OrganizationChannelCreationFailedEvent,
          OrganizationChannelUpdatedRequestedEvent = eventData.OrganizationChannelUpdatedRequestedEvent,
          OrganizationChannelUpdatedEvent = eventData.OrganizationChannelUpdatedEvent,
          OrganizationChannelUpdateFailedEvent = eventData.OrganizationChannelUpdateFailedEvent,
          OrganizationChannelDeletionRequestedEvent = eventData.OrganizationChannelDeletionRequestedEvent,
          OrganizationChannelDeletedEvent = eventData.OrganizationChannelDeletedEvent,
          OrganizationChannelDeletionFailedEvent = eventData.OrganizationChannelDeletionFailedEvent
        });
      } catch (Exception ex) {
        _logger.LogError(ex, "Failed to send event data to client");
      }
    }
  }
}

/// <summary>
/// テスト用のダミーアカウントクラス
/// このクラスは、テストやモックの目的で使用されます。
/// </summary>
public class FakeAccount {
  public string Id { get; set; } = "fake-account-id";
  public string Name { get; set; } = "Fake Account";
  public string Email { get; set; } = "fake@email.com";
  /// <summary>
  /// アカウントがサブスクライブする組織のリスト
  /// </summary>
  public List<string> ListenOrganizationEvents { get; set; } = [];
  /// <summary>
  /// アカウントがサブスクライブするメッセージのリスト
  /// </summary>
  public List<string> ListenMessageEvents { get; set; } = [];
  /// <summary>
  /// アカウントがサブスクライブするユーザーイベントのリスト
  /// </summary>
  public List<string> ListenUserEvents { get; set; } = [];
}
