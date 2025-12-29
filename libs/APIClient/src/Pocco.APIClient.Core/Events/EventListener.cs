using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Pocco.APIClient.Core.Models;
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

        try {
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
                    case ClientEvents.ON_ORGANIZATION_ROLE_CREATED:
                        _client.Logger.LogInformation("Organization role created event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var createdRolePerms = new List<string>();
                        createdRolePerms.AddRange(eventMessage.Payload.Fields["permissions"].StringValue.Split(","));
                        var createdRole = new Role {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            RoleId = eventMessage.Payload.Fields["role_id"].StringValue,
                            Name = eventMessage.Payload.Fields["name"].StringValue,
                            // description
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        };
                        createdRole.Permissions.AddRange(createdRolePerms);

                        _client.EventHub.Push(new ClientEvents.OnOrganizationRoleCreated(
                            eventMessage.EventId,
                            createdRole
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_ROLE_UPDATED:
                        _client.Logger.LogInformation("Organization role created event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var updatedRolePerms = new List<string>();
                        updatedRolePerms.AddRange(eventMessage.Payload.Fields["permissions"].StringValue.Split(","));
                        var uppdatedRole = new Role {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            RoleId = eventMessage.Payload.Fields["role_id"].StringValue,
                            Name = eventMessage.Payload.Fields["name"].StringValue,
                            // description
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        };
                        uppdatedRole.Permissions.AddRange(updatedRolePerms);

                        _client.EventHub.Push(new ClientEvents.OnOrganizationRoleUpdated(
                            eventMessage.EventId,
                            uppdatedRole
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_ROLE_DELETED:
                        _client.Logger.LogInformation("Organization role created event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        _client.EventHub.Push(new ClientEvents.OnOrganizationRoleDeleted(
                            eventMessage.EventId,
                            new OrganizationItemDeletedModel(
                                eventMessage.Payload.Fields["organization_id"].StringValue,
                                eventMessage.Payload.Fields["role_id"].StringValue
                            )
                        ));
                        break;

                    case ClientEvents.ON_ORGANIZATION_CHAT_CREATED:
                        _client.Logger.LogInformation("Organization chat created event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var createdChat = new Chat {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            ChatId = eventMessage.Payload.Fields["chat_id"].StringValue,
                            Name = eventMessage.Payload.Fields["name"].StringValue,
                            Description = eventMessage.Payload.Fields["description"].StringValue,
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        };

                        _client.EventHub.Push(new ClientEvents.OnOrganizationChatCreated(
                            eventMessage.EventId,
                            createdChat
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_CHAT_UPDATED:
                        _client.Logger.LogInformation("Organization chat updated event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var updatedChat = new Chat {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            ChatId = eventMessage.Payload.Fields["chat_id"].StringValue,
                            Name = eventMessage.Payload.Fields["name"].StringValue,
                            Description = eventMessage.Payload.Fields["description"].StringValue,
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        };

                        _client.EventHub.Push(new ClientEvents.OnOrganizationChatUpdated(
                            eventMessage.EventId,
                            updatedChat
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_CHAT_DELETED:
                        _client.Logger.LogInformation("Organization chat deleted event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        _client.EventHub.Push(new ClientEvents.OnOrganizationChatDeleted(
                            eventMessage.EventId,
                            new OrganizationItemDeletedModel(
                                eventMessage.Payload.Fields["organization_id"].StringValue,
                                eventMessage.Payload.Fields["chat_id"].StringValue
                            )
                        ));
                        break;

                    case ClientEvents.ON_ORGANIZATION_MEMBER_JOINED:
                        _client.Logger.LogInformation("Organization member joined event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var joinedMember = new Member {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            UserId = eventMessage.Payload.Fields["id"].StringValue,
                            JoinedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["joined_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["joined_at"].StringValue).ToUniversalTime()), // TODO: Send updated_at data
                        };
                        var joinedMemberRoles = new List<string>();
                        joinedMemberRoles.AddRange(eventMessage.Payload.Fields["roles"].StringValue.Split(","));

                        _client.EventHub.Push(new ClientEvents.OnOrganizationMemberCreated(
                            eventMessage.EventId,
                            joinedMember
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_MEMBER_LEAVED:
                        _client.Logger.LogInformation("Organization member leaved event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        _client.EventHub.Push(new ClientEvents.OnOrganizationMemberDeleted(
                            eventMessage.EventId,
                            new OrganizationItemDeletedModel(
                                eventMessage.Payload.Fields["organization_id"].StringValue,
                                eventMessage.Payload.Fields["id"].StringValue
                            )
                        ));
                        break;

                    case ClientEvents.ON_ORGANIZATION_MESSAGE_CREATED:
                        _client.Logger.LogInformation("Organization message created event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var createdMessage = new Message {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            ChatId = eventMessage.Payload.Fields["chat_id"].StringValue,
                            MessageId = eventMessage.Payload.Fields["message_id"].StringValue,
                            SenderId = eventMessage.Payload.Fields["sender_id"].StringValue,
                            Content = eventMessage.Payload.Fields["content"].StringValue,
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        };

                        _client.EventHub.Push(new ClientEvents.OnOrganizationSendMessage(
                            eventMessage.EventId,
                            createdMessage
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_MESSAGE_UPDATED:
                        _client.Logger.LogInformation("Organization message updated event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        var updatedMessage = new Message {
                            OrganizationId = eventMessage.Payload.Fields["organization_id"].StringValue,
                            ChatId = eventMessage.Payload.Fields["chat_id"].StringValue,
                            MessageId = eventMessage.Payload.Fields["message_id"].StringValue,
                            SenderId = eventMessage.Payload.Fields["sender_id"].StringValue,
                            Content = eventMessage.Payload.Fields["content"].StringValue,
                            CreatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["created_at"].StringValue).ToUniversalTime()),
                            UpdatedAt = Timestamp.FromDateTime(DateTime.Parse(eventMessage.Payload.Fields["updated_at"].StringValue).ToUniversalTime()),
                        };

                        _client.EventHub.Push(new ClientEvents.OnOrganizationUpdateMessage(
                            eventMessage.EventId,
                            updatedMessage
                        ));
                        break;
                    case ClientEvents.ON_ORGANIZATION_MESSAGE_DELETED:
                        _client.Logger.LogInformation("Organization message deleted event received: {EventType} on {OrgId}", eventMessage.EventType, eventMessage.Payload.Fields["organization_id"].StringValue);

                        _client.EventHub.Push(new ClientEvents.OnOrganizationDeleteMessage(
                            eventMessage.EventId,
                            new OrganizationOnItemDeletedModel(
                                eventMessage.Payload.Fields["organization_id"].StringValue,
                                eventMessage.Payload.Fields["chat_id"].StringValue,
                                eventMessage.Payload.Fields["message_id"].StringValue
                            )
                        ));
                        break;

                    default:
                        _client.Logger.LogWarning("Unhandled event type: {EventType}", eventMessage.EventType);
                        break;
                }
            }
        } catch (Exception ex) {
            _client.Logger.LogError("EventListener error ocured: {Error}", ex);
        }

        _client.Logger.LogInformation("EventListener stopped listening for events.");
        _listeningTask = null;
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
