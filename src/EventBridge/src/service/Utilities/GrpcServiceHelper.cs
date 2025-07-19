using Pocco.Svc.EventBridge.Protobufs.Services;

namespace Pocco.Svc.EventBridge.Utilities;

public static class GrpcServiceHelper {
  public enum EventCategory {
    Account,
    Organization,
    Message,
    Other
  }

  public static EventCategory GetEventCategory(V0EventData.PayloadOneofCase eventType) {
    return eventType switch {
      // Account関連のイベント
      V0EventData.PayloadOneofCase.AccountCreatedEvent or
      V0EventData.PayloadOneofCase.AccountUpdatedEvent or
      V0EventData.PayloadOneofCase.AccountModeratedEvent or
      V0EventData.PayloadOneofCase.AccountDisabledEvent => EventCategory.Account,

      // Organization関連のイベント
      V0EventData.PayloadOneofCase.MessageCreatedEvent or
      V0EventData.PayloadOneofCase.MessageUpdatedEvent or
      V0EventData.PayloadOneofCase.MessageDeletedEvent or
      V0EventData.PayloadOneofCase.MessageReactionAddedEvent or
      V0EventData.PayloadOneofCase.MessageReactionRemovedEvent => EventCategory.Organization,

      // その他のイベント
      _ => EventCategory.Other
    };
  }

  public static object GetEventData(V0EventData eventData) {
    return eventData.PayloadCase switch {
      // Account関連のイベントデータ
      V0EventData.PayloadOneofCase.AccountCreatedEvent => eventData.AccountCreatedEvent,
      V0EventData.PayloadOneofCase.AccountUpdatedEvent => eventData.AccountUpdatedEvent,
      V0EventData.PayloadOneofCase.AccountModeratedEvent => eventData.AccountModeratedEvent,
      V0EventData.PayloadOneofCase.AccountDisabledEvent => eventData.AccountDisabledEvent,
      // Organization関連のイベントデータ
      V0EventData.PayloadOneofCase.MessageCreatedEvent => eventData.MessageCreatedEvent,
      V0EventData.PayloadOneofCase.MessageUpdatedEvent => eventData.MessageUpdatedEvent,
      V0EventData.PayloadOneofCase.MessageDeletedEvent => eventData.MessageDeletedEvent,
      V0EventData.PayloadOneofCase.MessageReactionAddedEvent => eventData.MessageReactionAddedEvent,
      V0EventData.PayloadOneofCase.MessageReactionRemovedEvent => eventData.MessageReactionRemovedEvent,

      // その他のイベントデータは例外
      _ => throw new ArgumentException("Unsupported event data type")
    };
  }
}
