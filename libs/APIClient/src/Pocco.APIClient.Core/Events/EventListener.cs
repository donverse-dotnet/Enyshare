using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.Libs.Protobufs.CoreAPI.Services;

namespace Pocco.APIClient.Core.Events;

public class EventListener : IDisposable {
    public ListenRequest CurrentListeningEvents => _currentListeningEvents;

    private readonly APIClient _client;
    private readonly V0EventsService.V0EventsServiceClient _eventListener;
    private ListenRequest _currentListeningEvents = new();
    private Task? _listeningTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private CancellationToken _cancellationToken;

    public EventListener(APIClient client) {
        _client = client;
        _cancellationToken = _cancellationTokenSource.Token;

        var channel = GrpcChannel.ForAddress(_client.APIEndpoint);
        _eventListener = new V0EventsService.V0EventsServiceClient(channel);

        _client.Logger.LogInformation("EventListener initialized.");
    }

    public async Task StartListeningAsync(ListenRequest? request = null) {
        _client.Logger.LogInformation("EventListener started listening for events.");

        if (_listeningTask == null || _listeningTask.IsCompleted) {
            _listeningTask = Task.Run(async () => await ListenForEventsAsync(request), _cancellationToken);
        } else {
            _client.Logger.LogWarning("EventListener is already listening for events.");
        }

        await Task.CompletedTask;
    }


    public async Task UpdateSubscriptionAsync(ListenRequest data) {
        var header = _client.SessionManager.GetSessionData()?.ToMetadata() ?? [];
        await _eventListener.UpdateListenAsync(data, header, cancellationToken: _cancellationToken);

        _client.Logger.LogInformation("Subscribed to event type: {EventType}", data);
        _currentListeningEvents = data;
    }

    private async Task ListenForEventsAsync(ListenRequest? request = null) {
        if (request is null) {
            _client.Logger.LogInformation("No ListenRequest provided. Listening to all events.");
        } else {
            _client.Logger.LogInformation("Listening to events with request: {Request}", request);
        }

        var listenEvents = request ?? new ListenRequest();
        var header = _client.SessionManager.GetSessionData()?.ToMetadata() ?? [];
        using var call = _eventListener.Listen(listenEvents, header, cancellationToken: _cancellationToken);

        _currentListeningEvents = listenEvents;
        _client.Logger.LogInformation("EventListener is now listening these events: {Events}", _currentListeningEvents);

        await foreach (var eventMessage in call.ResponseStream.ReadAllAsync(_cancellationToken)) {
            // 受信したイベントメッセージの処理をここに実装
            switch (eventMessage.EventType) {
                case ClientEvents.ON_ORGANIZATION_INFO_CREATED:
                    _client.Logger.LogInformation("Organization created event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["info_id"].StringValue);
                    _currentListeningEvents.OrganizationIds.Add(eventMessage.Payload.Fields["info_id"].StringValue);
                    await UpdateSubscriptionAsync(_currentListeningEvents);

                    _client.EventHub.Push(new ClientEvents.OnOrganizationInfoCreated(
                        eventMessage.EventId,
                        new Organization {
                            OrganizationId = eventMessage.Payload.Fields["info_id"].StringValue,
                            Name = eventMessage.Payload.Fields["name"].StringValue,
                            Description = eventMessage.Payload.Fields["description"].StringValue,
                            CreatedBy = eventMessage.Payload.Fields["created_by"].StringValue,
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        }
                    ));
                    break;
                case ClientEvents.ON_ORGANIZATION_INFO_UPDATED:
                    _client.Logger.LogInformation("Organization name updated event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["info_id"].StringValue);

                    _client.EventHub.Push(new ClientEvents.OnOrganizationInfoUpdated(
                        eventMessage.EventId,
                        new Organization {
                            OrganizationId = eventMessage.Payload.Fields["info_id"].StringValue,
                            Name = eventMessage.Payload.Fields["name"].StringValue,
                            Description = eventMessage.Payload.Fields["description"].StringValue,
                            CreatedBy = eventMessage.Payload.Fields["created_by"].StringValue,
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        }
                    ));
                    break;
                case ClientEvents.ON_ORGANIZATION_INFO_DELETED:
                    _client.Logger.LogInformation("Organization deleted event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["info_id"].StringValue);
                    _currentListeningEvents.OrganizationIds.Remove(eventMessage.Payload.Fields["info_id"].StringValue);
                    await UpdateSubscriptionAsync(_currentListeningEvents);

                    _client.EventHub.Push(new ClientEvents.OnOrganizationInfoDeleted(
                        eventMessage.EventId,
                        eventMessage.Payload.Fields["info_id"].StringValue
                    ));
                    break;
                // 未実装はこれより下にbreakなしで追加

                default:
                    _client.Logger.LogWarning("Unhandled event type: {EventType}", eventMessage.EventType);
                    break;
            }
        }

        await Task.CompletedTask;
    }

    public void Dispose() {
        _client.Logger.LogInformation("Disposing EventListener...");

        _cancellationTokenSource.Cancel();
        _listeningTask?.Wait();
        _cancellationTokenSource.Dispose();

        _client.Logger.LogInformation("EventListener disposed.");
        GC.SuppressFinalize(this);
    }
}
