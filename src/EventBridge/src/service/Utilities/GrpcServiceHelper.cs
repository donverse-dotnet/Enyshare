namespace Pocco.Svc.EventBridge.Utilities;

public static class GrpcServiceHelper {
  public enum EventCategory {
    Account,
    Organization,
    Message,
    Other
  }

  public static EventCategory GetEventCategory(DeployEventRequest.EventDataOneofCase eventType) {
    return eventType switch {
      // Account関連のイベント
      DeployEventRequest.EventDataOneofCase.AccountCreationRequestedEvent or
      DeployEventRequest.EventDataOneofCase.AccountCreatedEvent or
      DeployEventRequest.EventDataOneofCase.AccountCreationFailedEvent or
      DeployEventRequest.EventDataOneofCase.AccountUpdatedRequestedEvent or
      DeployEventRequest.EventDataOneofCase.AccountUpdatedEvent or
      DeployEventRequest.EventDataOneofCase.AccountUpdateFailedEvent or
      DeployEventRequest.EventDataOneofCase.AccountDeletionRequestedEvent or
      DeployEventRequest.EventDataOneofCase.AccountDeletedEvent or
      DeployEventRequest.EventDataOneofCase.AccountDeletionFailedEvent => EventCategory.Account,

      // Organization関連のイベント
      DeployEventRequest.EventDataOneofCase.OrganizationCreatedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationCreationFailedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationUpdatedRequestedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationUpdatedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationUpdateFailedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationDeletionRequestedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationDeletedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationDeletionFailedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelCreationRequestedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelCreatedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelCreationFailedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelUpdatedRequestedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelUpdatedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelUpdateFailedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelDeletionRequestedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelDeletedEvent or
      DeployEventRequest.EventDataOneofCase.OrganizationChannelDeletionFailedEvent => EventCategory.Organization,

      // Message関連のイベント
      DeployEventRequest.EventDataOneofCase.MessageCreatedEvent or
      DeployEventRequest.EventDataOneofCase.MessageUpdatedEvent or
      DeployEventRequest.EventDataOneofCase.MessageDeletedEvent => EventCategory.Message,

      // その他のイベント
      _ => EventCategory.Other
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
