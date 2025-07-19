using System.Threading.Channels;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Utilities;

namespace Pocco.Svc.EventBridge.Services.Handlers;

// TODO: Rename to EventDispatcher
// TODO: Fix event data handling to avoid multiple assignments

public class EventDispatcher : IDisposable {
  public readonly IAsyncEnumerable<KeyValuePair<string, V0EventData>> EventQueue = _eventChannel.Reader.ReadAllAsync();
  public readonly Dictionary<string, IServerStreamWriter<V0EventData>> ClientList = [];

  private static readonly Channel<KeyValuePair<string, V0EventData>> _eventChannel = Channel.CreateUnbounded<KeyValuePair<string, V0EventData>>();
  private readonly MongoClient _mongoClient;
  private readonly ILogger<EventDispatcher> _logger;
  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private readonly CancellationToken _cancellationToken;

  public EventDispatcher(
    [FromServices] MongoClient mongoClient,
    [FromServices] ILogger<EventDispatcher> logger
  ) {
    // Initialize the event queue channel
    _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _cancellationToken = _cancellationTokenSource.Token;

    // Start the event queue processing
    Task.Run(async () => {
      await InvokeEventQueue(_cancellationToken);
    });
  }

  public void Dispose() {
    _eventChannel.Writer.TryComplete();
    _cancellationTokenSource.Cancel();
    _cancellationTokenSource.Dispose();
    _logger.LogInformation("EventSender disposed and event channel completed.");
  }

  private async Task InvokeEventQueue(CancellationToken cancellationToken = default) {
    await foreach (var eventData in EventQueue) {
      await Task.Run(async () => {
        var (eventId, data) = eventData;
        var clients = SelectTargetClients(data.PayloadCase, eventId);
        if (clients.Count > 0) {
          if (data is null) {
            _logger.LogWarning("Event data is null for event ID: {EventId}", eventId);
            return;
          }

          await SendToAffectedClientsAsync(clients, eventId);
        }
      });
    }
  }


  // public class EventData(string eventId, EventData.PayloadOneofCase eventType, DeployEventRequest data) {
  //   public readonly string EventId = eventId;
  //   public readonly DeployEventRequest.EventDataOneofCase EventType = eventType;
  //   public readonly DeployEventRequest Data = data;
  // }

  public async Task AddClientAsync(
    string accountId,
    IServerStreamWriter<V0EventData> responseStream
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
    V0EventData eventData
  ) {
    if (string.IsNullOrEmpty(eventId) || eventData is null) {
      _logger.LogError("Invalid event data or event ID: {EventId}", eventId);
      return false;
    }

    var eventType = eventData.PayloadCase;
    if (eventType == V0EventData.PayloadOneofCase.None) {
      _logger.LogError("Event data type is unset for event ID: {EventId}", eventId);
      return false;
    }
    var eventDataObj = new object() as V0EventData;
    var success = _eventChannel.Writer.TryWrite(new KeyValuePair<string, V0EventData>(eventId, eventDataObj));
    if (!success) {
      _logger.LogError("Failed to write event data to channel for event ID: {EventId}", eventId);
      return false;
    }

    await Task.CompletedTask;
    return true;
  }

  public List<IServerStreamWriter<V0EventData>> SelectTargetClients(V0EventData.PayloadOneofCase eventType, string id) {
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

  public async Task SendToAffectedClientsAsync(List<IServerStreamWriter<V0EventData>> clients, string eventId) {
    foreach (var client in clients) {
      try {
        await client.WriteAsync(new V0EventData {
          // TODO: イベントデータは複数代入してはいけないので、処理を分散させる
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
