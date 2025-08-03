using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs.Services;

namespace Pocco.Svc.EventBridge.Services.Handlers;

public class V0EventHandlerImpl(
  [FromServices] EventDispatcher eventDispatcher,
  [FromServices] ILogger<EventHandler> logger
) : V0EventHandler.V0EventHandlerBase {
  public override async Task Subscribe(V0SubscribeRequest request, IServerStreamWriter<V0EventData> responseStream, ServerCallContext context) {
    // ストリームライターをクライアントとして登録
    await eventDispatcher.AddClientAsync(request.SessionId, responseStream);

    // ストリームがキャンセルもしくは完了した場合、クライアントを削除
    context.CancellationToken.Register(async () => {
      await eventDispatcher.RemoveClientAsync(request.SessionId);
      logger.LogInformation("Client {SessionId} unsubscribed.", request.SessionId);
    });
  }

  public override async Task<Empty> Unsubscribe(V0UnsubscribeRequest request, ServerCallContext context) {
    // クライアントを削除
    await eventDispatcher.RemoveClientAsync(request.SessionId);
    logger.LogInformation("Client {SessionId} unsubscribed.", request.SessionId);
    return new Empty();
  }
}
