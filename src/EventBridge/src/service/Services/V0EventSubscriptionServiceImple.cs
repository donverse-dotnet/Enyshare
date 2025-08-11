using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Services;

namespace Pocco.Svc.EventBridge.Services;

public class V0EventSubscriptionServiceImpl(
  [FromServices] V0EventInvoker eventInvoker,
  ILogger<V0EventSubscriptionServiceImpl> logger
) : V0EventHandler.V0EventHandlerBase {
  public override async Task Subscribe(V0SubscribeRequest request, IServerStreamWriter<V0EventData> responseStream, ServerCallContext context) {
    var clientAdded = eventInvoker.TryAddClient(request.SessionId, request.AccountId, responseStream, context);
    if (!clientAdded) {
      logger.LogWarning("Failed to add client for session {SessionId} and account {AccountId}", request.SessionId, request.AccountId);
      throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid session or account ID"));
    }

    logger.LogInformation("Client added for session {SessionId} and account {AccountId}", request.SessionId, request.AccountId);

    // Start processing the event queue for this client
    while (!context.CancellationToken.IsCancellationRequested) {
      await Task.Delay(100); // Polling delay to avoid busy waiting
    }

    await Task.CompletedTask;
  }

  public override async Task<Empty> Unsubscribe(V0UnsubscribeRequest request, ServerCallContext context) {
    var clientRemoved = eventInvoker.TryRemoveClient(request.SessionId);
    if (!clientRemoved) {
      logger.LogWarning("Failed to remove client for session {SessionId}", request.SessionId);
      throw new RpcException(new Status(StatusCode.NotFound, "Session not found"));
    }

    logger.LogInformation("Client removed for session {SessionId}", request.SessionId);
    return await Task.FromResult(new Empty());
  }
}
