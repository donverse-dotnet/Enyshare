
using System.Threading.Channels;
using Grpc.Core;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.Svc.CoreAPI.Models;

public class StreamWriterModel {

  public StreamWriterModel(string sessionId, string userId, StreamWriterFilterModel filters, IServerStreamWriter<V0EventData> streamWriter, ServerCallContext context, ILogger logger) {
    SessionId = sessionId;
    UserId = userId;
    Filters = filters;
    StreamWriter = streamWriter;
    StreamContext = context;
    Logger = logger;

    // Start processing the event queue
    _processingTask = Task.Run(async () => await ProcessEventQueueAsync(_cancellationTokenSource.Token));
  }
  ~StreamWriterModel() {
    // Clean up
    _cancellationTokenSource.Cancel();
    _processingTask.Wait();
    _cancellationTokenSource.Dispose();
  }

  public ILogger Logger { get; init; }

  public string SessionId { get; init; }
  public string UserId { get; init; }

  public StreamWriterFilterModel Filters { get; private set; }

  public IServerStreamWriter<V0EventData> StreamWriter { get; init; }
  public ServerCallContext StreamContext { get; init; }

  private static readonly Channel<V0EventData> _channel = System.Threading.Channels.Channel.CreateUnbounded<V0EventData>();
  private readonly IAsyncEnumerable<V0EventData> _eventQueue = _channel.Reader.ReadAllAsync();
  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private readonly Task _processingTask;

  public void EnqueueEvent(V0EventData eventData) {
    var writed = _channel.Writer.TryWrite(eventData);

    if (!writed) {
      Logger.LogWarning("Failed to enqueue event for SessionId={SessionId}, UserId={UserId}", SessionId, UserId);
    }
  }

  private async Task ProcessEventQueueAsync(CancellationToken cancellationToken) {
    await foreach (var eventData in _eventQueue.WithCancellation(cancellationToken)) {
      Logger.LogInformation("Sending event to SessionId={SessionId}, UserId={UserId}: Topic={Topic}, Payload={Payload}",
        SessionId, UserId, eventData.Topic, eventData.Payload);

      await StreamWriter.WriteAsync(eventData, cancellationToken);

      Logger.LogInformation("Event sent to SessionId={SessionId}, UserId={UserId}: Topic={Topic}, Payload={Payload}",
        SessionId, UserId, eventData.Topic, eventData.Payload);
    }
  }
}
