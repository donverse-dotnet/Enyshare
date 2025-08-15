using System.Threading.Channels;
using Grpc.Core;
using Pocco.Svc.EventBridge.Protobufs.Services;

namespace Pocco.Svc.EventBridge.Services;

public class Clients {
  /// <summary>
  /// クライアントのセッションID。
  /// <para>このIDは、クライアントがイベントを受信するための識別子として使用されます。</para>
  /// <para>セッションIDは、クライアントが接続している間に一意であり、切断された場合は再接続時に新しいIDが生成されます。</para>
  /// <para>このIDを使用して、特定のクライアントにイベントを送信することができます。</para>
  /// <para>例: "session12345"</para>
  /// </summary>
  private string _sessionId = string.Empty;
  /// <summary>
  /// クライアントのアカウントID。
  /// <para>このIDは、クライアントが属するアカウントを識別するために使用されます。</para>
  /// <para>アカウントIDは、クライアントがイベントを受信するためのフィルタリングに使用されます。</para>
  /// <para>例: "account12345"</para>
  /// </summary>
  private string _accountId = string.Empty;

  /// <summary>
  /// クライアントの組織IDリスト。
  /// <para>キーは組織ID、値はアクティブなターゲットかどうかを示すブール値です。</para>
  /// <para>アクティブであれば、詳細なイベントを受信することができます。アクティブでない場合は、イベントが発生したことのみが通知されます。</para>
  /// </summary>
  private Dictionary<string, bool> _organizationIdList = [];

  private IServerStreamWriter<V0EventData> _writer = null!;
  private ServerCallContext _context = null!;

  public string GetSessionId() {
    return _sessionId;
  }

  public Clients SetSessionId(string sessionId) {
    _sessionId = sessionId;
    return this;
  }

  public string GetAccountId() {
    return _accountId;
  }

  public Clients SetAccountId(string accountId) {
    _accountId = accountId;
    return this;
  }

  public Dictionary<string, bool> GetOrganizationIdList() {
    return _organizationIdList;
  }

  public Clients SetOrganizationIdList(Dictionary<string, bool> organizationIdList) {
    _organizationIdList = organizationIdList;
    return this;
  }

  public IServerStreamWriter<V0EventData> GetWriter() {
    return _writer;
  }

  public Clients SetWriter(IServerStreamWriter<V0EventData> writer) {
    _writer = writer;
    return this;
  }

  public ServerCallContext GetContext() {
    return _context;
  }

  public Clients SetContext(ServerCallContext context) {
    _context = context;
    return this;
  }
}

public partial class V0EventInvoker {
  /// <summary>
  /// イベントを送信するためのサーバーストリームライターのリスト。
  /// キーはセッションID、値はキーがアカウントID、値がサーバーストリームライターのペアです。
  /// </summary>
  public List<Clients> ClientList { get; } = [];

  private static readonly Channel<V0EventData> _eventQueueAdapter = Channel.CreateUnbounded<V0EventData>();
  private static int _maxEventExecutionSize { get; set; } = 1000;
  private DateTime _before { get; set; } = DateTime.UtcNow;

  public SemaphoreSlim EventExecutionLimiter { get; } = new(_maxEventExecutionSize);
  public List<Task> EventExecutingTasks { get; } = [];
  private readonly ILogger<V0EventInvoker> _logger;

  public V0EventInvoker(ILogger<V0EventInvoker> logger) {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    // イベントキューの処理を開始
    Task.Run(ExecuteEventsAsync);
  }

  public bool AddToEventQueue(V0EventData e) {
    if (_eventQueueAdapter.Writer.TryWrite(e)) {
      _logger.LogInformation("Event added to queue: {EventId}", e.BaseEvent.EventId);
      return true;
    } else {
      _logger.LogWarning("Failed to add event to queue: {EventId}", e.BaseEvent.EventId);
      return false;
    }
  }

  public List<string> CreateTargetForAccountEventsFilter(V0EventData payload) {
    switch (payload.PayloadCase) {
      case V0EventData.PayloadOneofCase.AccountCreatedEvent:
        // セッションIDを取得
        var ace = ClientList
          .Where(c => c.GetAccountId() == payload.AccountCreatedEvent.Id)
          .Select(c => c.GetSessionId())
          .ToList();
        _logger.LogInformation("Found {Count} clients for AccountCreatedEvent with ID: {payload.AccountCreatedEvent.Id}", ace.Count, payload.BaseEvent.EventId);
        return ace;
      case V0EventData.PayloadOneofCase.AccountUpdatedEvent:
        // セッションIDを取得
        var aue = ClientList
          .Where(c => c.GetAccountId() == payload.AccountUpdatedEvent.AccountModel.AccountId)
          .Select(c => c.GetSessionId())
          .ToList();
        _logger.LogInformation("Found {Count} clients for AccountCreatedEvent with ID: {payload.AccountCreatedEvent.Id}", aue.Count, payload.BaseEvent.EventId);
        return aue;
      case V0EventData.PayloadOneofCase.AccountModeratedEvent:
        // セッションIDを取得
        var ame = ClientList
          .Where(c => c.GetAccountId() == payload.AccountModeratedEvent.AccountId)
          .Select(c => c.GetSessionId())
          .ToList();
        _logger.LogInformation("Found {Count} clients for AccountCreatedEvent with ID: {payload.AccountCreatedEvent.Id}", ame.Count, payload.BaseEvent.EventId);
        return ame;
      case V0EventData.PayloadOneofCase.AccountDisabledEvent:
        // セッションIDを取得
        var ade = ClientList
          .Where(c => c.GetAccountId() == payload.AccountDisabledEvent.AccountId)
          .Select(c => c.GetSessionId())
          .ToList();
        _logger.LogInformation("Found {Count} clients for AccountCreatedEvent with ID: {payload.AccountCreatedEvent.Id}", ade.Count, payload.BaseEvent.EventId);
        return ade;
      default:
        _logger.LogWarning("Unknown event type: {PayloadCase}", payload.PayloadCase);
        return [];
    }
  }

  public async Task ProcessEventQueueAsync(V0EventData e) {
    // Create filter for client affected by the event
    var filter = e.PayloadCase switch {
      V0EventData.PayloadOneofCase.AccountCreatedEvent => CreateTargetForAccountEventsFilter(e),
      V0EventData.PayloadOneofCase.AccountUpdatedEvent => CreateTargetForAccountEventsFilter(e),
      V0EventData.PayloadOneofCase.AccountModeratedEvent => CreateTargetForAccountEventsFilter(e),
      V0EventData.PayloadOneofCase.AccountDisabledEvent => CreateTargetForAccountEventsFilter(e),
      _ => []
    };
    // Get clients with client filter
    var clients = ClientList
      .Where(c => filter.Contains(c.GetSessionId()))
      .ToList();
    // Send event to each client
    foreach (var client in clients) {
      try {
        _logger.LogInformation("Sending event {EventId} to client {ClientId}", e.BaseEvent.EventId, client.GetAccountId());
        await client.GetWriter().WriteAsync(e);
      } catch (Exception ex) {
        _logger.LogError(ex, "Failed to send event {EventId} to client {ClientId}", e.BaseEvent.EventId, client.GetAccountId());
        // If the client is disconnected, remove it from the list
        if (TryRemoveClient(client.GetAccountId())) {
          _logger.LogInformation("Removed client {ClientId} after failed send", client.GetAccountId());
        }
      }
    }

    await Task.CompletedTask;
  }

  public async Task ExecuteEventsAsync() {
    _logger.LogInformation("Starting event execution at {Time}", DateTime.UtcNow);

    await foreach (var data in _eventQueueAdapter.Reader.ReadAllAsync()) {
      await EventExecutionLimiter.WaitAsync();

      var task = Task.Run(async () => {
        try {
          _logger.LogInformation("Executing event: {EventId}", data.BaseEvent.EventId);
          await ProcessEventQueueAsync(data);
        } catch (Exception ex) {
          _logger.LogError(ex, "Error processing event: {EventId}", data.BaseEvent.EventId);
        } finally {
          EventExecutionLimiter.Release();
        }
      });

      EventExecutingTasks.Add(task);
      EventExecutingTasks.RemoveAll(t => t.IsCompleted);
    }

    await Task.CompletedTask;
  }

  public bool TryAddClient(string sessionId, string accountId, IServerStreamWriter<V0EventData> writer, ServerCallContext context) {
    if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(accountId) || writer == null) {
      return false;
    }

    // 既に同じセッションIDのクライアントが存在する場合は更新
    var existingClient = ClientList.FirstOrDefault(c => c.GetAccountId() == accountId);
    if (existingClient != null) {
      existingClient.SetWriter(writer);
      existingClient.SetContext(context);
      _logger.LogInformation("Updated existing client for account {AccountId} with session {SessionId}", accountId, sessionId);
      return true;
    }
    return true;
  }

  public bool TryRemoveClient(string sessionId) {
    if (string.IsNullOrEmpty(sessionId) || !ClientList.Any(c => c.GetAccountId() == sessionId)) {
      return false;
    }
    // 該当するクライアントを検索
    _logger.LogInformation("Removing client with session {SessionId}", sessionId);
    var clientToRemove = ClientList.FirstOrDefault(c => c.GetAccountId() == sessionId);
    if (clientToRemove == null) {
      _logger.LogWarning("No client found with session {SessionId}", sessionId);
      return false;
    }
    // セッションを切断
    clientToRemove.GetContext().CancellationToken.ThrowIfCancellationRequested();

    // 該当するクライアントを検索して削除
    if (clientToRemove == null) {
      _logger.LogWarning("No client found with session {SessionId}", sessionId);
      return false;
    }

    // クライアントを削除
    return ClientList.Remove(clientToRemove);
  }
}
