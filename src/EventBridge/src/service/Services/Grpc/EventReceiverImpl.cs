using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;

namespace Pocco.Svc.EventBridge.Services.Grpc;

public class EventReceiverImpl : V0EventReceiver.V0EventReceiverBase {
  public EventReceiverImpl([FromServices] EventSendHelper eventSendHelper, [FromServices] ILogger<EventReceiverImpl> logger) {
    _eventSendHelper = eventSendHelper;
    _logger = logger;
    _logger.LogInformation("EventReceiverImpl initialized");
  }

  private readonly EventSendHelper _eventSendHelper;
  private readonly ILogger<EventReceiverImpl> _logger;

  public override async Task<Empty> NewEvent(V0EventData request, ServerCallContext context) {
    _logger.LogInformation("Received new event: EventId={EventId}, EventType={EventType}", request.EventId, request.EventType);
    await _eventSendHelper.EnqueueEventAsync(request);

    _logger.LogInformation("Event enqueued successfully: EventId={EventId}", request.EventId);
    return new Empty();
  }
}
