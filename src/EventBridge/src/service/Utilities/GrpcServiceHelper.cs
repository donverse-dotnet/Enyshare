namespace Pocco.Svc.EventBridge.Utilities;

public static class GrpcServiceHelper {
  public static object GetEventData(SubscribeEventStreamData eventData) {
    return eventData.EventDataCase switch {
      // Account関連のイベントデータ
      SubscribeEventStreamData.EventDataOneofCase.AccountCreationRequestedEvent => eventData.AccountCreationRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountCreatedEvent => eventData.AccountCreatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountCreationFailedEvent => eventData.AccountCreationFailedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountUpdatedRequestedEvent => eventData.AccountUpdatedRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountUpdatedEvent => eventData.AccountUpdatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountUpdateFailedEvent => eventData.AccountUpdateFailedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountDeletionRequestedEvent => eventData.AccountDeletionRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountDeletedEvent => eventData.AccountDeletedEvent,
      SubscribeEventStreamData.EventDataOneofCase.AccountDeletionFailedEvent => eventData.AccountDeletionFailedEvent,
      // Organization関連のイベントデータ
      SubscribeEventStreamData.EventDataOneofCase.OrganizationCreatedEvent => eventData.OrganizationCreatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationCreationFailedEvent => eventData.OrganizationCreationFailedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationUpdatedRequestedEvent => eventData.OrganizationUpdatedRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationUpdatedEvent => eventData.OrganizationUpdatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationUpdateFailedEvent => eventData.OrganizationUpdateFailedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationDeletionRequestedEvent => eventData.OrganizationDeletionRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationDeletedEvent => eventData.OrganizationDeletedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationDeletionFailedEvent => eventData.OrganizationDeletionFailedEvent,
      // Message関連のイベントデータ
      SubscribeEventStreamData.EventDataOneofCase.MessageCreatedEvent => eventData.MessageCreatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.MessageUpdatedEvent => eventData.MessageUpdatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.MessageDeletedEvent => eventData.MessageDeletedEvent,
      // OrganizationChannel関連のイベントデータ
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelCreationRequestedEvent => eventData.OrganizationChannelCreationRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelCreatedEvent => eventData.OrganizationChannelCreatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelCreationFailedEvent => eventData.OrganizationChannelCreationFailedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelUpdatedRequestedEvent => eventData.OrganizationChannelUpdatedRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelUpdatedEvent => eventData.OrganizationChannelUpdatedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelUpdateFailedEvent => eventData.OrganizationChannelUpdateFailedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelDeletionRequestedEvent => eventData.OrganizationChannelDeletionRequestedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelDeletedEvent => eventData.OrganizationChannelDeletedEvent,
      SubscribeEventStreamData.EventDataOneofCase.OrganizationChannelDeletionFailedEvent => eventData.OrganizationChannelDeletionFailedEvent,
      _ => throw new ArgumentException("Unsupported event data type")
    };
  }

  public static object GetEventData(DeployEventRequest eventData) {
    return eventData.EventDataCase switch {
      // Account関連のイベントデータ
      DeployEventRequest.EventDataOneofCase.AccountCreationRequestedEvent => eventData.AccountCreationRequestedEvent,
      DeployEventRequest.EventDataOneofCase.AccountCreatedEvent => eventData.AccountCreatedEvent,
      DeployEventRequest.EventDataOneofCase.AccountCreationFailedEvent => eventData.AccountCreationFailedEvent,
      DeployEventRequest.EventDataOneofCase.AccountUpdatedRequestedEvent => eventData.AccountUpdatedRequestedEvent,
      DeployEventRequest.EventDataOneofCase.AccountUpdatedEvent => eventData.AccountUpdatedEvent,
      DeployEventRequest.EventDataOneofCase.AccountUpdateFailedEvent => eventData.AccountUpdateFailedEvent,
      DeployEventRequest.EventDataOneofCase.AccountDeletionRequestedEvent => eventData.AccountDeletionRequestedEvent,
      DeployEventRequest.EventDataOneofCase.AccountDeletedEvent => eventData.AccountDeletedEvent,
      DeployEventRequest.EventDataOneofCase.AccountDeletionFailedEvent => eventData.AccountDeletionFailedEvent,
      // Organization関連のイベントデータ
      DeployEventRequest.EventDataOneofCase.OrganizationCreatedEvent => eventData.OrganizationCreatedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationCreationFailedEvent => eventData.OrganizationCreationFailedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationUpdatedRequestedEvent => eventData.OrganizationUpdatedRequestedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationUpdatedEvent => eventData.OrganizationUpdatedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationUpdateFailedEvent => eventData.OrganizationUpdateFailedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationDeletionRequestedEvent => eventData.OrganizationDeletionRequestedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationDeletedEvent => eventData.OrganizationDeletedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationDeletionFailedEvent => eventData.OrganizationDeletionFailedEvent,
      // Message関連のイベントデータ
      DeployEventRequest.EventDataOneofCase.MessageCreatedEvent => eventData.MessageCreatedEvent,
      DeployEventRequest.EventDataOneofCase.MessageUpdatedEvent => eventData.MessageUpdatedEvent,
      DeployEventRequest.EventDataOneofCase.MessageDeletedEvent => eventData.MessageDeletedEvent,
      // OrganizationChannel関連のイベントデータ
      DeployEventRequest.EventDataOneofCase.OrganizationChannelCreationRequestedEvent => eventData.OrganizationChannelCreationRequestedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelCreatedEvent => eventData.OrganizationChannelCreatedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelCreationFailedEvent => eventData.OrganizationChannelCreationFailedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelUpdatedRequestedEvent => eventData.OrganizationChannelUpdatedRequestedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelUpdatedEvent => eventData.OrganizationChannelUpdatedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelUpdateFailedEvent => eventData.OrganizationChannelUpdateFailedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelDeletionRequestedEvent => eventData.OrganizationChannelDeletionRequestedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelDeletedEvent => eventData.OrganizationChannelDeletedEvent,
      DeployEventRequest.EventDataOneofCase.OrganizationChannelDeletionFailedEvent => eventData.OrganizationChannelDeletionFailedEvent,
      _ => throw new ArgumentException("Unsupported event data type")
    };
  }
}
