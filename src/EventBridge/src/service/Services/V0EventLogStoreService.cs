using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Pocco.Svc.EventBridge.Services.Models;

namespace Pocco.Svc.EventBridge.Services;

public class V0EventLogStoreService : DbContext {
  private readonly ILogger<V0EventLogStoreService> _logger;

  /// <summary>
  /// イベントログをキャッシュするためのチャネル。
  /// </summary>
  public static readonly Channel<V0EventLogModel> EventQueueChannel = Channel.CreateUnbounded<V0EventLogModel>();
  /// <summary>
  /// イベントログの非同期イテレータ。
  /// イベントログのリストを非同期に読み取ることができます。
  /// </summary>
  public readonly IAsyncEnumerable<V0EventLogModel> EventQueue = EventQueueChannel.Reader.ReadAllAsync();

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
    EventQueueChannel.Writer.TryComplete();
    _cancellationTokenSource.Cancel();
    _cancellationTokenSource.Dispose();
    base.Dispose();
    _logger.LogInformation("V0EventLogStoreService disposed and event queue completed.");

    GC.SuppressFinalize(this);
  }

  private async Task InvokeEventSaveQueue() {
    await foreach (var eventLog in EventQueue) {
      try {
        // イベントログをデータベースに追加
        EventLogs.Add(eventLog);
        // 変更を保存
        await SaveChangesAsync();

        _logger.LogInformation("Event log saved: {EventId}", eventLog.EventId);
      } catch (Exception ex) {
        _logger.LogError(ex, "Failed to save event log: {EventId}", eventLog.EventId);
      }
    }
  }
}
