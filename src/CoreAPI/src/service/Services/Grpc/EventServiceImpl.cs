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
    var filters = new StreamWriterFilterModel(
      topics: request.Topics.Select(t => t.ToString()).ToList(),
      organizationIds: request.OrganizationIds.ToList(),
      activeOrganizationId: request.ActiveOrganizationId,
      activeOrganizationChatIds: request.DirectMessageIds.ToList()
    );

    var streamWriterModel = new StreamWriterModel(
      sessionId: request.SessionId,
      userId: request.UserId,
      filters: filters, // TODO: 1つのフィルターになるように変更
      streamWriter: responseStream,
      context: context,
      logger: _logger
    );

    // Register the StreamWriterModel
    _streamHolder.AddStreamWriter(streamWriterModel);

    // Wait until the client disconnects
    try {
      while (!context.CancellationToken.IsCancellationRequested) {
        Task.Delay(100, context.CancellationToken).Wait(); // Keep the connection alive
      }
    } catch (TaskCanceledException) {
      // Client disconnected
      _logger.LogInformation("Client disconnected for SessionId={SessionId}", request.SessionId);
    } catch (OperationCanceledException) {
      // Client disconnect requested
      _logger.LogInformation("Operation canceled for SessionId={SessionId}", request.SessionId);
    } finally {
      // Unregister the StreamWriterModel
      _logger.LogInformation("Removing StreamWriter for SessionId={SessionId}", request.SessionId);
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
      foreach (var topic in request.Topics) {
        if (!streamWriterModel.Filters.Topics.Contains(topic.ToString())) {
          streamWriterModel.Filters.Topics.Add(topic.ToString());
        }
      }
      foreach (var orgId in request.OrganizationIds) {
        if (!streamWriterModel.Filters.OrganizationIds.Contains(orgId)) {
          streamWriterModel.Filters.OrganizationIds.Add(orgId);
        }
      }
      streamWriterModel.Filters.UpdateActiveOrganizationId(request.ActiveOrganizationId);
      streamWriterModel.Filters.UpdateActiveOrganizationChatIds(request.DirectMessageIds.ToList());

      _logger.LogInformation("Updated StreamWriterModel: SessionId={SessionId}, UserId={UserId}, Filters={Filters}",
        request.SessionId, request.UserId, string.Join(",", streamWriterModel.Filters.ToString()));
    } else {
      _logger.LogWarning("StreamWriterModel not found for SessionId={SessionId}", request.SessionId);
    }

    return await Task.FromResult(new Empty());
  }
}
