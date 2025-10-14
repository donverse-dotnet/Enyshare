// Implement gRPC register event listener
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Pocco.Libs.Protobufs.Services;
using Pocco.Svc.CoreAPI.Models;

namespace Pocco.Svc.CoreAPI.Services.Grpc;

public class EventServiceImpl : V0EventsService.V0EventsServiceBase {
  public EventServiceImpl([FromServices] ILogger<EventServiceImpl> logger, [FromServices] StreamHolder streamHolder) {
    _logger = logger;
    _streamHolder = streamHolder;

    _logger.LogInformation("EventServiceImpl initialized.");
  }

  private readonly ILogger<EventServiceImpl> _logger;
  private readonly StreamHolder _streamHolder;

  public override async Task Listen(ListenRequest request, IServerStreamWriter<V0EventData> responseStream, ServerCallContext context) {
    // Create a new StreamWriterModel for this connection
    var topics = new List<string>();
    foreach (var topic in request.Topics) {
      topics.Add(topic.ToString());
    }
    var filters = new List<string>();
    filters.AddRange(request.OrganizationIds);
    filters.AddRange(request.DirectMessageIds);
    filters.Add(request.ActiveOrganizationId);

    var streamWriterModel = new StreamWriterModel(
      sessionId: request.SessionId,
      userId: request.UserId,
      topics: topics,
      filters: filters, // TODO: 1つのフィルターになるように変更
      streamWriter: responseStream,
      context: context
    );

    // Register the StreamWriterModel
    _streamHolder.AddStreamWriter(streamWriterModel);

    // Wait until the client disconnects
    try {
      while (!context.CancellationToken.IsCancellationRequested) {
        await Task.Delay(100, context.CancellationToken); // Keep the connection alive
      }
    } catch (TaskCanceledException) {
      // Client disconnected
      _logger.LogInformation("Client disconnected for SessionId={SessionId}", request.SessionId);
    } catch (OperationCanceledException) {
      // Client disconnect requested
      _logger.LogInformation("Operation canceled for SessionId={SessionId}", request.SessionId);
    } finally {
      // Unregister the StreamWriterModel
      _streamHolder.RemoveStreamWriter(request.SessionId);
    }

    _logger.LogInformation("Client disconnected: SessionId={SessionId}, UserId={UserId}", request.SessionId, request.UserId);

    await Task.CompletedTask;
  }

  public override async Task<Empty> UpdateListen(ListenRequest request, ServerCallContext context) {
    // Update the StreamWriterModel for this connection
    var streamWriterModels = _streamHolder.GetStreamWriters(wr => wr.SessionId == request.SessionId);

    if (streamWriterModels.Count == 0) {
      _logger.LogWarning("No StreamWriterModel found for SessionId={SessionId}", request.SessionId);
      throw new RpcException(new Status(StatusCode.NotFound, $"The session's writer is not found for {request.SessionId}."));
    }
    var streamWriterModel = streamWriterModels.First();

    if (streamWriterModel != null) {
      var topics = new List<string>();
      foreach (var topic in request.Topics) {
        topics.Add(topic.ToString());
      }
      var filters = new List<string>();
      filters.AddRange(request.OrganizationIds);
      filters.AddRange(request.DirectMessageIds);
      filters.Add(request.ActiveOrganizationId);

      streamWriterModel.UpdateTopic(topics);
      streamWriterModel.UpdateFilters(filters); // TODO: 1つのフィルターになるように変更

      _logger.LogInformation("Updated StreamWriterModel: SessionId={SessionId}, UserId={UserId}, Topics={Topics}, Filters={Filters}",
        request.SessionId, request.UserId, string.Join(",", topics), string.Join(",", filters));
    } else {
      _logger.LogWarning("StreamWriterModel not found for SessionId={SessionId}", request.SessionId);
    }

    return await Task.FromResult(new Empty());
  }
}
