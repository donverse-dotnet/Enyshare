using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

using Pocco.Svc.EventBridge.Protobufs.Enums;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;
using Pocco.Svc.Messages.Protobufs.Services;
using Pocco.Svc.Messages.Protobufs.Types;

namespace MessageService.Services;

public class OrganizationMessageGrpcService(
    [FromServices] DatabaseManager dbs,
    [FromServices] ILogger<OrganizationMessageGrpcService> logger,
    [FromServices] V0EventReceiver.V0EventReceiverClient eventBridge
) : OrganizationMessageRpcService.OrganizationMessageRpcServiceBase {
  private readonly DatabaseManager dbManager = dbs;
  private readonly ILogger<OrganizationMessageGrpcService> _logger = logger;
  private readonly V0EventReceiver.V0EventReceiverClient _eventBridge = eventBridge;

  public override Task<V0TryMessageActionResponse> TrySendMessageToOrganization(V0TrySendMessageToOrganizationRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    var currentTime = Timestamp.FromDateTime(DateTime.UtcNow);
    // Create a new message
    var newMessage = new V0Messages {
      Id = MessageIdGenerator.GenerateOrganizationMessageId(),
      ChatId = request.ChatId,
      SenderId = request.SenderId,
      Content = request.Content,
      Attachments = request.Attachments,
      Mentions = request.Mentions,
      Reactions = request.Reactions,
      CreatedAt = currentTime,
      UpdatedAt = currentTime,
      IsDeleted = false
    };

    // Insert the new message into the collection
    messagesCollection.InsertOne(newMessage);

    // Check if the message was successfully inserted
    if (messagesCollection.Find(m => m.Id == newMessage.Id).Any() is false) {
      _logger.LogError("Failed to insert message into organization: {OrgId}, ChatId: {ChatId}", request.OrganizationId, request.ChatId);
      throw new RpcException(new Status(StatusCode.Internal, "Failed to insert message into organization."));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationSendMessage",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.SenderId,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrganizationId}" });
    newEventData.Payload.Fields.Add("message_id", new Value { StringValue = $"{newMessage.Id}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{newMessage.ChatId}" });
    newEventData.Payload.Fields.Add("sender_id", new Value { StringValue = $"{newMessage.SenderId}" });
    newEventData.Payload.Fields.Add("content", new Value { StringValue = $"{newMessage.Content}" });
    newEventData.Payload.Fields.Add("attachments", new Value { StringValue = $"{newMessage.Attachments}" });
    newEventData.Payload.Fields.Add("mentions", new Value { StringValue = $"{newMessage.Mentions}" });
    newEventData.Payload.Fields.Add("reactions", new Value { StringValue = $"{newMessage.Reactions}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{newMessage.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{newMessage.UpdatedAt}" });

    var createdEventData = _eventBridge.NewEvent(
      newEventData
    );

    return Task.FromResult(new V0TryMessageActionResponse {
      MessageSentEventId = createdEventData.EventId
    });
  }
  public override async Task<V0TryMessageActionResponse> TryUpdateMessageInOrganization(V0TryUpdateMessageInOrganizationRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    // Find the message to update
    var filter = Builders<V0Messages>.Filter.Eq(m => m.Id, request.MessageId);
    var update = Builders<V0Messages>.Update
        .Set(m => m.Content, request.Content)
        .Set(m => m.UpdatedAt, Timestamp.FromDateTime(DateTime.UtcNow));

    // Update the message
    var result = await messagesCollection.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      _logger.LogError("Failed to update message in organization: {OrgId}, ChatId: {ChatId}, MessageId: {MessageId}", request.OrganizationId, request.ChatId, request.MessageId);
      throw new RpcException(new Status(StatusCode.NotFound, "Message not found or not updated."));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationUpdateMessage",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.SenderId,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrganizationId}" });
    newEventData.Payload.Fields.Add("message_id", new Value { StringValue = $"{request.MessageId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{request.ChatId}" });
    newEventData.Payload.Fields.Add("sender_id", new Value { StringValue = $"{request.SenderId}" });
    newEventData.Payload.Fields.Add("content", new Value { StringValue = $"{request.Content}" });
    newEventData.Payload.Fields.Add("attachments", new Value { StringValue = $"{request.Attachments}" });
    newEventData.Payload.Fields.Add("mentions", new Value { StringValue = $"{request.Mentions}" });
    newEventData.Payload.Fields.Add("reactions", new Value { StringValue = $"{request.Reactions}" });
    newEventData.Payload.Fields.Add("created_at", new Value { StringValue = $"{request.CreatedAt}" });
    newEventData.Payload.Fields.Add("updated_at", new Value { StringValue = $"{request.UpdatedAt}" });


    var updatedEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0TryMessageActionResponse {
      MessageSentEventId = updatedEventData.EventId
    };
  }

  public override async Task<V0TryMessageActionResponse> TryDeleteMessageFromOrganization(V0TryDeleteMessageFromOrganizationRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    // Find the message to delete
    var filter = Builders<V0Messages>.Filter.Eq(m => m.Id, request.MessageId);

    // Update the message to mark it as deleted
    var update = Builders<V0Messages>.Update.Set(m => m.IsDeleted, true);
    var result = await messagesCollection.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      _logger.LogError("Failed to delete message in organization: {OrgId}, ChatId: {ChatId}, MessageId: {MessageId}", request.OrganizationId, request.ChatId, request.MessageId);
      throw new RpcException(new Status(StatusCode.NotFound, "Message not found or not deleted."));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationDeleteMessage",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.SenderId,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrganizationId}" });
    newEventData.Payload.Fields.Add("message_id", new Value { StringValue = $"{request.ChatId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{request.ChatId}" });

    var deletedEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0TryMessageActionResponse {
      MessageSentEventId = deletedEventData.EventId
    };
  }
  public override async Task<V0TryMessageActionResponse> TryAddReactionToMessage(V0TryAddReactionToMessageRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"chat_{request.ChatId}");

    // Find the message to delete
    var filter = Builders<V0Messages>.Filter.Eq(m => m.Id, request.MessageId);

    // Update the message to mark it as deleted
    var update = Builders<V0Messages>.Update.Set(m => m.IsDeleted, true);
    var result = await messagesCollection.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      _logger.LogError("Failed to delete message in organization: {OrgId}, ChatId: {ChatId}, MessageId: {MessageId}", request.OrganizationId, request.ChatId, request.MessageId);
      throw new RpcException(new Status(StatusCode.NotFound, "Message not found or not deleted."));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationAddReactionMessage",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.SenderId,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrganizationId}" });
    newEventData.Payload.Fields.Add("message_id", new Value { StringValue = $"{request.MessageId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{request.ChatId}" });
    newEventData.Payload.Fields.Add("reactions", new Value { StringValue = $"{request.Reaction}" });


    var addReactionEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0TryMessageActionResponse {
      MessageSentEventId = addReactionEventData.EventId
    };
  }
  public override async Task<V0TryMessageActionResponse> TryRemoveReactionFromMessage(V0TryRemoveReactionFromMessageRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    // Find the message to update
    var filter = Builders<V0Messages>.Filter.Eq(m => m.Id, request.MessageId);
    // Get the current message
    var message = messagesCollection.Find(filter).FirstOrDefault()
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Message not found."));

    // Update the message
    var update = Builders<V0Messages>.Update.Pull(m => m.Reactions.Reactions, request.Reaction);
    var result = await messagesCollection.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      _logger.LogError("Failed to remove reaction from message in organization: {OrgId}, ChatId: {ChatId}, MessageId: {MessageId}", request.OrganizationId, request.ChatId, request.MessageId);
      throw new RpcException(new Status(StatusCode.NotFound, "Message not found or reaction not removed."));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationRemoveReactionMessage",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.SenderId,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrganizationId}" });
    newEventData.Payload.Fields.Add("message_id", new Value { StringValue = $"{request.MessageId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{request.ChatId}" });
    newEventData.Payload.Fields.Add("reactions", new Value { StringValue = $"{request.Reaction}" });


    var removeReactionEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0TryMessageActionResponse {
      MessageSentEventId = removeReactionEventData.EventId
    };
  }
  public override async Task<V0TryMessageActionResponse> TryBulkRemoveReactionsFromMessage(V0TryBulkRemoveReactionsFromMessageRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    // Find the message to update
    var filter = Builders<V0Messages>.Filter.Eq(m => m.Id, request.MessageId);
    // Get the current message
    var message = messagesCollection.Find(filter).FirstOrDefault()
        ?? throw new RpcException(new Status(StatusCode.NotFound, "Message not found."));

    // Update the message
    var update = Builders<V0Messages>.Update.PullAll(m => m.Reactions.Reactions, request.Reactions);
    var result = await messagesCollection.UpdateOneAsync(filter, update);

    if (result.ModifiedCount == 0) {
      _logger.LogError("Failed to bulk remove reactions from message in organization: {OrgId}, ChatId: {ChatId}, MessageId: {MessageId}", request.OrganizationId, request.ChatId, request.MessageId);
      throw new RpcException(new Status(StatusCode.NotFound, "Message not found or reactions not removed."));
    }

    var newEventData = new V0NewEventRequest {
      Topic = V0EventTopics.EventTopicOrganization,
      EventType = "OnOrganizationBulkRemoveReactionMessage",
      ApiVersion = "0",
      InvokedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      InvokedBy = request.SenderId,
      Payload = new Struct()
    };

    newEventData.Payload.Fields.Add("organization_id", new Value { StringValue = $"{request.OrganizationId}" });
    newEventData.Payload.Fields.Add("message_id", new Value { StringValue = $"{request.MessageId}" });
    newEventData.Payload.Fields.Add("chat_id", new Value { StringValue = $"{request.ChatId}" });
    newEventData.Payload.Fields.Add("reactions", new Value { StringValue = $"{request.Reactions}" });


    var bulkremoveReactionEventData = _eventBridge.NewEvent(
      newEventData
    );

    return new V0TryMessageActionResponse {
      MessageSentEventId = bulkremoveReactionEventData.EventId
    };
  }

  public override async Task<V0GetMessageInOrganizationResponse> GetMessageInOrganization(V0GetMessageInOrganizationRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    // Find the message
    var filter = Builders<V0Messages>.Filter.Eq(m => m.Id, request.MessageId);
    var message = await messagesCollection.Find(filter).FirstOrDefaultAsync();

    if (message is null) {
      _logger.LogError("Message not found in organization: {OrgId}, ChatId: {ChatId}, MessageId: {MessageId}", request.OrganizationId, request.ChatId, request.MessageId);
      throw new RpcException(new Status(StatusCode.NotFound, "Message not found."));
    }

    return new V0GetMessageInOrganizationResponse {
      Message = message
    };
  }

  public override async Task<V0GetMessageListInOrganizationResponse> GetMessageListInOrganization(V0GetMessageListInOrganizationRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"{request.ChatId}");

    // Build the filter
    var filter = Builders<V0Messages>.Filter.Empty;
    if (!string.IsNullOrEmpty(request.FirstMessageId)) {
      filter = Builders<V0Messages>.Filter.Gt(m => m.Id, request.FirstMessageId);
    }

    // Retrieve the messages
    var messages = await messagesCollection.Find(filter)
        .SortBy(m => m.Id)
        .Limit(request.PageSize)
        .ToListAsync();

    var response = new V0GetMessageListInOrganizationResponse();
    response.Messages.AddRange(messages);

    return response;
  }
}
