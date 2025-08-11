using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Driver;

using Pocco.Svc.Messages.Protobufs.Services;
using Pocco.Svc.Messages.Protobufs.Types;

namespace MessageService.Services;

public class OrganizationMessageGrpcService(
    [FromServices] DatabaseManager dbs,
    [FromServices] ILogger<OrganizationMessageGrpcService> logger
) : OrganizationMessageRpcService.OrganizationMessageRpcServiceBase {
  private readonly DatabaseManager dbManager = dbs;
  private readonly ILogger<OrganizationMessageGrpcService> _logger = logger;

  public override Task<V0TryMessageActionResponse> TrySendMessageToOrganization(V0TrySendMessageToOrganizationRequest request, ServerCallContext context) {
    // Get organization database client
    var _mongoOrg = dbManager.GetDatabaseClient(request.OrganizationId);
    if (_mongoOrg is null) {
      _logger.LogError("MongoDB client is not initialized for organization: {OrgId}", request.OrganizationId);
      throw new RpcException(new Status(StatusCode.Internal, "MongoDB client is not initialized."));
    }

    // Get messages collection for the organization
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"chat_{request.ChatId}");

    // Create a new message
    var newMessage = new V0Messages {
      Id = MessageIdGenerator.GenerateOrganizationMessageId(),
      ChatId = request.ChatId,
      SenderId = request.SenderId,
      Content = request.Content,
      Attachments = request.Attachments,
      Mentions = request.Mentions,
      Reactions = request.Reactions,
      CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
      IsDeleted = false
    };

    // Insert the new message into the collection
    messagesCollection.InsertOne(newMessage);

    // Check if the message was successfully inserted
    if (messagesCollection.Find(m => m.Id == newMessage.Id).Any() is false) {
      _logger.LogError("Failed to insert message into organization: {OrgId}, ChatId: {ChatId}", request.OrganizationId, request.ChatId);
      throw new RpcException(new Status(StatusCode.Internal, "Failed to insert message into organization."));
    }

    // Call event bridge method
    // var eventId = eventBridge.PublishOrganizationMessageCreatedEvent(newMessage, request.OrganizationId);

    return Task.FromResult(new V0TryMessageActionResponse {
      // MessageSentEventId = eventId
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
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"chat_{request.ChatId}");

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

    // Call event bridge method
    // var eventId = eventBridge.PublishOrganizationMessageCreatedEvent(newMessage, request.OrganizationId);

    return new V0TryMessageActionResponse {
      // MessageUpdatedEventId = eventId
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

    // Call event bridge method
    // var eventId = eventBridge.PublishOrganizationMessageDeletedEvent(request.MessageId, request.OrganizationId);

    return new V0TryMessageActionResponse {
      // MessageDeletedEventId = eventId
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

    // Call event bridge method
    // var eventId = eventBridge.PublishOrganizationMessageReactionAddedEvent(request.MessageId, request.Reaction, request.OrganizationId);

    return new V0TryMessageActionResponse {
      // MessageReactionAddedEventId = eventId
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
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"chat_{request.ChatId}");

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

    // Call event bridge method
    // var eventId = eventBridge.PublishOrganizationMessageReactionRemovedEvent(request.MessageId, request.Reaction, request.OrganizationId);

    return new V0TryMessageActionResponse {
      // MessageReactionRemovedEventId = eventId
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
    var messagesCollection = _mongoOrg.GetCollection<V0Messages>($"chat_{request.ChatId}");

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

    // Call event bridge method
    // var eventId = eventBridge.PublishOrganizationMessageReactionsRemovedEvent(request.MessageId, request.Reactions, request.OrganizationId);

    return new V0TryMessageActionResponse {
      // MessageReactionBulkRemovedEventId = eventId
    };
  }
}
