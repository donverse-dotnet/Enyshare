using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services.Grpc;

public class EventDispatcherImpl : V0EventDispatcher.V0EventDispatcherBase {
  public EventDispatcherImpl([FromServices] EventSendHelper eventSendHelper, [FromServices] ILogger<EventDispatcherImpl> logger) {
    _eventSendHelper = eventSendHelper;
    _logger = logger;
    _logger.LogInformation("EventDispatcherImpl initialized");
  }

  private readonly EventSendHelper _eventSendHelper;
  private readonly ILogger<EventDispatcherImpl> _logger;

  public override async Task Listen(ListenRequest request, IServerStreamWriter<V0DeployedEventData> responseStream, ServerCallContext context) {
    _logger.LogInformation("Client connected to ListenEvents");

    var uuid = _eventSendHelper.AddListener(responseStream);

    try {
      // Keep the stream open until the client disconnects
      while (!context.CancellationToken.IsCancellationRequested) {
        await Task.Delay(1000, context.CancellationToken);
      }
    } catch (OperationCanceledException) {
      // Expected when the client disconnects
      _logger.LogInformation("Client disconnected from ListenEvents");
    } catch (Exception ex) {
      _logger.LogError(ex, "Error in ListenEvents");
    } finally {
      _eventSendHelper.RemoveListener(uuid);
      _logger.LogInformation("Listener removed: {Uuid}", uuid);
    }
  }
}
