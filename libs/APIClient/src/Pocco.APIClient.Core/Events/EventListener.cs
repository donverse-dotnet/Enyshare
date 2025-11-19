using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.Services;

namespace Pocco.APIClient.Core.Events;

public class EventListener : IDisposable {
    private readonly APIClient _client;
    private readonly V0EventsService.V0EventsServiceClient _eventListener;
    private Task? _listeningTask;
    private CancellationToken _cancellationToken;

    public EventListener(APIClient client, CancellationToken cancellationToken = default) {
        _client = client;
        _cancellationToken = cancellationToken;

        var channel = GrpcChannel.ForAddress(_client.APIEndpoint);
        _eventListener = new V0EventsService.V0EventsServiceClient(channel);

        _client.Logger.LogInformation("EventListener initialized.");
    }

    public async Task StartListeningAsync() {
        _client.Logger.LogInformation("EventListener started listening for events.");

        if (_listeningTask == null || _listeningTask.IsCompleted) {
            _listeningTask = ListenForEventsAsync();
        } else {
            _client.Logger.LogWarning("EventListener is already listening for events.");
        }

        await Task.CompletedTask;
    }

    public async Task UpdateSubscriptionAsync(ListenRequest data) {
        _client.Logger.LogInformation("Subscribed to event type: {EventType}", data);

        var header = _client.SessionManager.GetSessionData()?.ToMetadata() ?? [];
        await _eventListener.UpdateListenAsync(data, header, cancellationToken: _cancellationToken);
    }

    private async Task ListenForEventsAsync() {
        var header = _client.SessionManager.GetSessionData()?.ToMetadata() ?? [];
        using var call = _eventListener.Listen(new ListenRequest(), header, cancellationToken: _cancellationToken);

        await foreach (var eventMessage in call.ResponseStream.ReadAllAsync(_cancellationToken)) {
            _client.Logger.LogInformation("Received event: {EventMessage}", eventMessage);
            // 受信したイベントメッセージの処理をここに実装
            switch (eventMessage.EventType) {
                // 未実装はこれより下にbreakなしで追加
                case ClientEvents.ON_ORGANIZATION_CREATED:
                case ClientEvents.ON_ORGANIZATION_NAME_UPDATED:
                case ClientEvents.ON_ORGANIZATION_DELETED:

                default:
                    _client.Logger.LogWarning("Unhandled event type: {EventType}", eventMessage.EventType);
                    break;
            }
        }

        await Task.CompletedTask;
    }

    public void Dispose() {
        _client.Logger.LogInformation("Disposing EventListener...");

        _listeningTask?.Wait();

        GC.SuppressFinalize(this);
    }
}
