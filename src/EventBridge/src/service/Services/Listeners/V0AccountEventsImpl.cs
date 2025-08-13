using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.EventBridge.Protobufs;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Pocco.Svc.EventBridge.Protobufs.Types;
using Pocco.Svc.EventBridge.Services.Models;

namespace Pocco.Svc.EventBridge.Services;

public class V0AccountEventsImpl(
  [FromServices] V0EventInvoker eventInvoker,
  [FromServices] V0EventLogStoreService eventLogStoreService,
  [FromServices] ILogger<V0AccountEventsImpl> logger
) : V0AccountEvents.V0AccountEventsBase {
  private readonly V0EventInvoker _v0EventInvoker = eventInvoker;
  private readonly V0EventLogStoreService _v0EventLogStoreService = eventLogStoreService;
  private readonly ILogger<V0AccountEventsImpl> _logger = logger;

  public override async Task<V0PublishResponse> V0PublishAccountCreated(V0AccountCreatedEvent request, ServerCallContext context) {
    // イベントIDを生成
    var eId = "e_" + Guid.NewGuid().ToString("N");

    try {
      // イベントデータを作成して、イベントキューに追加
      var eventData = new V0EventData {
        BaseEvent = new V0BaseEvent {
          EventId = eId,
          FiredAt = Timestamp.FromDateTime(DateTime.UtcNow),
        },
        AccountCreatedEvent = request
      };
      var eventQueued = _v0EventInvoker.AddToEventQueue(eventData);

      if (!eventQueued) {
        throw new InvalidOperationException("Failed to queue event data.");
      }

      // ログストアにイベントを保存
      var eventLogModel = new V0EventLogModel {
        EventId = eId,
        EventType = (int)eventData.PayloadCase,
        FiredAt = DateTime.UtcNow,
        EventData = request.ToString() // Serialize the request to a string
      };
      await V0EventLogStoreService.EventQueueChannel.Writer.WriteAsync(eventLogModel);

      // ログにイベントの発行を記録
      _logger.LogInformation("Published V0AccountCreatedEvent: {EventId}", eId);

      // 成功レスポンスを返す
      return new V0PublishResponse {
        Success = true,
        EventId = eId,
        ErrorMessage = string.Empty
      };
    } catch (Exception ex) {
      // エラーが発生した場合はログに記録し、失敗レスポンスを返す
      _logger.LogError(ex, "Failed to publish V0AccountCreatedEvent: {EventId}", eId);
      return new V0PublishResponse {
        Success = false,
        EventId = eId,
        ErrorMessage = ex.Message
      };
    }
  }

  public override async Task<V0PublishResponse> V0PublishAccountUpdated(V0AccountUpdatedEvent request, ServerCallContext context) {
    // イベントIDを生成
    var eId = "e_" + Guid.NewGuid().ToString("N");

    try {
      // イベントデータを作成して、イベントキューに追加
      var eventData = new V0EventData {
        BaseEvent = new V0BaseEvent {
          EventId = eId,
          FiredAt = Timestamp.FromDateTime(DateTime.UtcNow),
        },
        AccountUpdatedEvent = request
      };
      var eventQueued = _v0EventInvoker.AddToEventQueue(eventData);

      if (!eventQueued) {
        throw new InvalidOperationException("Failed to queue event data.");
      }

      // ログストアにイベントを保存
      var eventLogModel = new V0EventLogModel {
        EventId = eId,
        EventType = (int)eventData.PayloadCase,
        FiredAt = DateTime.UtcNow,
        EventData = request.ToString() // Serialize the request to a string
      };
      await V0EventLogStoreService.EventQueueChannel.Writer.WriteAsync(eventLogModel);

      // ログにイベントの発行を記録
      _logger.LogInformation("Published V0AccountUpdatedEvent: {EventId}", eId);

      // 成功レスポンスを返す
      return new V0PublishResponse {
        Success = true,
        EventId = eId,
        ErrorMessage = string.Empty
      };
    } catch (Exception ex) {
      // エラーが発生した場合はログに記録し、失敗レスポンスを返す
      _logger.LogError(ex, "Failed to publish V0AccountUpdatedEvent: {EventId}", eId);
      return new V0PublishResponse {
        Success = false,
        EventId = eId,
        ErrorMessage = ex.Message
      };
    }
  }

  public override async Task<V0PublishResponse> V0PublishAccountModerated(V0AccountModeratedEvent request, ServerCallContext context) {
    // イベントIDを生成
    var eId = "e_" + Guid.NewGuid().ToString("N");

    try {
      // イベントデータを作成して、イベントキューに追加
      var eventData = new V0EventData {
        BaseEvent = new V0BaseEvent {
          EventId = eId,
          FiredAt = Timestamp.FromDateTime(DateTime.UtcNow),
        },
        AccountModeratedEvent = request
      };
      var eventQueued = _v0EventInvoker.AddToEventQueue(eventData);

      if (!eventQueued) {
        throw new InvalidOperationException("Failed to queue event data.");
      }

      // ログストアにイベントを保存
      var eventLogModel = new V0EventLogModel {
        EventId = eId,
        EventType = (int)eventData.PayloadCase,
        FiredAt = DateTime.UtcNow,
        EventData = request.ToString() // Serialize the request to a string
      };
      await V0EventLogStoreService.EventQueueChannel.Writer.WriteAsync(eventLogModel);

      // ログにイベントの発行を記録
      _logger.LogInformation("Published V0AccountModeratedEvent: {EventId}", eId);

      // 成功レスポンスを返す
      return new V0PublishResponse {
        Success = true,
        EventId = eId,
        ErrorMessage = string.Empty
      };
    } catch (Exception ex) {
      // エラーが発生した場合はログに記録し、失敗レスポンスを返す
      _logger.LogError(ex, "Failed to publish V0AccountModeratedEvent: {EventId}", eId);
      return new V0PublishResponse {
        Success = false,
        EventId = eId,
        ErrorMessage = ex.Message
      };
    }
  }

  public override async Task<V0PublishResponse> V0PublishAccountDisabled(V0AccountDisabledEvent request, ServerCallContext context) {
    // イベントIDを生成
    var eId = "e_" + Guid.NewGuid().ToString("N");

    try {
      // イベントデータを作成して、イベントキューに追加
      var eventData = new V0EventData {
        BaseEvent = new V0BaseEvent {
          EventId = eId,
          FiredAt = Timestamp.FromDateTime(DateTime.UtcNow),
        },
        AccountDisabledEvent = request
      };
      var eventQueued = _v0EventInvoker.AddToEventQueue(eventData);

      if (!eventQueued) {
        throw new InvalidOperationException("Failed to queue event data.");
      }

      // ログストアにイベントを保存
      var eventLogModel = new V0EventLogModel {
        EventId = eId,
        EventType = (int)eventData.PayloadCase,
        FiredAt = DateTime.UtcNow,
        EventData = request.ToString() // Serialize the request to a string
      };
      await V0EventLogStoreService.EventQueueChannel.Writer.WriteAsync(eventLogModel);

      // ログにイベントの発行を記録
      _logger.LogInformation("Published V0AccountDisabledEvent: {EventId}", eId);

      // 成功レスポンスを返す
      return new V0PublishResponse {
        Success = true,
        EventId = eId,
        ErrorMessage = string.Empty
      };
    } catch (Exception ex) {
      // エラーが発生した場合はログに記録し、失敗レスポンスを返す
      _logger.LogError(ex, "Failed to publish V0AccountDisabledEvent: {EventId}", eId);
      return new V0PublishResponse {
        Success = false,
        EventId = eId,
        ErrorMessage = ex.Message
      };
    }
  }
}
