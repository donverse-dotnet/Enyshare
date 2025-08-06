using System.Threading.Channels;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services;

public class Clients {
  public string AccountId { get; set; }
  public IServerStreamWriter<V0EventData> Writer { get; set; }
  public ServerCallContext Context { get; set; }

  public Clients(string accountId, IServerStreamWriter<V0EventData> writer, ServerCallContext context) {
    AccountId = accountId;
    Writer = writer;
    Context = context;
  }
}

public class V0EventInvoker {
  /// <summary>
  /// イベントを送信するためのサーバーストリームライターのリスト。
  /// キーはセッションID、値はキーがアカウントID、値がサーバーストリームライターのペアです。
  /// </summary>
  public List<Clients> ClientList { get; } = [];

  /// <summary>
  /// イベントをキューに追加するためのチャネル。
  /// イベントデータは非同期に読み取ることができます。
  /// </summary>
  public static readonly Channel<V0EventData> EventChannel = Channel.CreateUnbounded<V0EventData>();
  /// <summary>
  /// イベントキューの非同期イテレータ。
  /// イベントデータのリストを非同期に読み取ることができます
  /// </summary>
  public readonly IAsyncEnumerable<V0EventData> EventQueue = EventChannel.Reader.ReadAllAsync();

  private readonly ILogger<V0EventInvoker> _logger;

  public V0EventInvoker(ILogger<V0EventInvoker> logger) {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    // イベントキューの処理を開始
    Task.Run(InvokeEventQueue);
  }

  private async Task InvokeEventQueue() {
    await foreach (var eventData in EventQueue) {
      _logger.LogInformation("Processing event: {EventId}", eventData.BaseEvent.EventId);
      // 配布用のイベントデータを作成
      var filter = CreateClientFilter(eventData);
      // クライアントを取得
      if (filter.Count == 0) {
        _logger.LogWarning("No clients found for event: {EventId}", eventData.BaseEvent.EventId);
        continue;
      }

      _logger.LogInformation("Publishing event: {EventId} to {ClientCount} clients", eventData.BaseEvent.EventId, filter.Count);
      // フィルターに基づいてクライアントにイベントを送信
      foreach (var clientId in filter) {
        var client = ClientList.FirstOrDefault(c => c.AccountId == clientId);
        if (client != null) {
          try {
            _logger.LogInformation("Sending event {EventId} to client {ClientId}", eventData.BaseEvent.EventId, client.AccountId);
            await client.Writer.WriteAsync(eventData);
          } catch (Exception ex) {
            _logger.LogError(ex, "Failed to send event {EventId} to client {ClientId}", eventData.BaseEvent.EventId, client.AccountId);
            // クライアントが切断された場合はリストから削除
            if (TryRemoveClient(client.AccountId)) {
              _logger.LogInformation("Removed client {ClientId} after failed send", client.AccountId);
            } else {
              _logger.LogWarning("Failed to remove client {ClientId} after failed send", client.AccountId);
            }
          }
        } else {
          _logger.LogWarning("Client {ClientId} not found for event {EventId}", clientId, eventData.BaseEvent.EventId);
        }
      }
    }

    _logger.LogInformation("Event queue processing completed.");
  }

  private List<string> CreateClientFilter(V0EventData payload) {
    // イベントターゲットに基づいてクライアントのフィルタを作成
    switch (payload.PayloadCase) {
      case V0EventData.PayloadOneofCase.AccountCreatedEvent:
        // セッションIDを取得
        var sessionIds = ClientList
          .Where(c => c.AccountId == payload.AccountCreatedEvent.Id)
          .Select(c => c.AccountId)
          .ToList();
        _logger.LogInformation("Found {Count} clients for AccountCreatedEvent with ID: {payload.AccountCreatedEvent.Id}", sessionIds.Count, payload.BaseEvent.EventId);
        return sessionIds;
      default:
        _logger.LogWarning("Unknown event type: {PayloadCase}", payload.PayloadCase);
        return [];
    }
  }

  public bool TryAddClient(string sessionId, string accountId, IServerStreamWriter<V0EventData> writer, ServerCallContext context) {
    if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(accountId) || writer == null) {
      return false;
    }

    // 既に同じセッションIDのクライアントが存在する場合は更新
    var existingClient = ClientList.FirstOrDefault(c => c.AccountId == accountId);
    if (existingClient != null) {
      existingClient.Writer = writer;
      existingClient.Context = context;
      _logger.LogInformation("Updated existing client for account {AccountId} with session {SessionId}", accountId, sessionId);
      return true;
    }
    return true;
  }

  public bool TryRemoveClient(string sessionId) {
    if (string.IsNullOrEmpty(sessionId) || !ClientList.Any(c => c.AccountId == sessionId)) {
      return false;
    }
    // 該当するクライアントを検索
    _logger.LogInformation("Removing client with session {SessionId}", sessionId);
    var clientToRemove = ClientList.FirstOrDefault(c => c.AccountId == sessionId);
    if (clientToRemove == null) {
      _logger.LogWarning("No client found with session {SessionId}", sessionId);
      return false;
    }
    // セッションを切断
    clientToRemove.Context.CancellationToken.ThrowIfCancellationRequested();

    // 該当するクライアントを検索して削除
    if (clientToRemove == null) {
      _logger.LogWarning("No client found with session {SessionId}", sessionId);
      return false;
    }

    // クライアントを削除
    return ClientList.Remove(clientToRemove);
  }
}
