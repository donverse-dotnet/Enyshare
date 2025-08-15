using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Pocco.Svc.EventBridge.Services.Models;

namespace Pocco.Svc.EventBridge.Services;

public class V0EventLogStoreService : DbContext {
  private readonly ILogger<V0EventLogStoreService> _logger;
  private const int _maxConcurrentTask = 1000;
  private readonly SemaphoreSlim _storeExecutionLimiter = new(_maxConcurrentTask);
  private readonly List<Task> _storeExecutionTasks = [];

  /// <summary>
  /// イベントログをキャッシュするためのチャネル。
  /// </summary>
  private static readonly Channel<V0EventLogModel> _queueAdapter = Channel.CreateUnbounded<V0EventLogModel>();

  /// <summary>
  /// イベントログのデータベースセット。
  /// このセットは、イベントログの永続化に使用されます。
  /// </summary>
  public DbSet<V0EventLogModel> EventLogs { get; set; } = null!;

  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private readonly CancellationToken _cancellationToken;

  public V0EventLogStoreService(ILogger<V0EventLogStoreService> logger, DbContextOptions<V0EventLogStoreService> options) : base(options) {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _cancellationToken = _cancellationTokenSource.Token;
    // データベースの初期化
    Database.EnsureCreated();
    // イベントログの保存キューを開始
    Task.Run(InvokeEventSaveQueue, _cancellationToken);

    _logger.LogInformation("V0EventLogStoreService initialized and event save queue started.");
  }

  public override void Dispose() {
    _queueAdapter.Writer.TryComplete();
    _cancellationTokenSource.Cancel();
    _cancellationTokenSource.Dispose();
    base.Dispose();
    _logger.LogInformation("V0EventLogStoreService disposed and event queue completed.");

    GC.SuppressFinalize(this);
  }

  private async Task InvokeEventSaveQueue() {
    await foreach (var eventLog in _queueAdapter.Reader.ReadAllAsync(_cancellationToken)) {
      await _storeExecutionLimiter.WaitAsync(_cancellationToken);

      var task = Task.Run(async () => {
        try {
          await EventLogs.AddAsync(eventLog);
          await SaveChangesAsync();
        } catch (Exception ex) {
          _logger.LogError(ex, "Failed to save event log: {EventId}", eventLog.EventId);
        } finally {
          _storeExecutionLimiter.Release();
        }
      }, _cancellationToken);

      _storeExecutionTasks.Add(task);
      _storeExecutionTasks.RemoveAll(t => t.IsCompleted);
    }
  }

  public bool TryEnqueueEventLog(V0EventLogModel eventLog) {
    if (eventLog == null) {
      _logger.LogWarning("Attempted to enqueue a null event log.");
      return false;
    }

    if (_queueAdapter.Writer.TryWrite(eventLog)) {
      _logger.LogInformation("Event log enqueued successfully: {EventId}", eventLog.EventId);
      return true;
    } else {
      _logger.LogWarning("Failed to enqueue event log: {EventId}. Queue may be full.", eventLog.EventId);
      return false;
    }
  }
}
