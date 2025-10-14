// Store stream writer for event listeners

using Microsoft.AspNetCore.Mvc;
using Pocco.Svc.CoreAPI.Models;

namespace Pocco.Svc.CoreAPI.Services;

public class StreamHolder : IHotStartableService {
  private readonly List<StreamWriterModel> _streamWriters;
  private readonly ILogger<StreamHolder> _logger;

  public StreamHolder([FromServices] ILogger<StreamHolder> logger) {
    _logger = logger;
    _streamWriters = [];

    _logger.LogInformation("StreamHolder initialized.");
  }

  // ユーザーがListenを読んだときに呼ぶ
  public void AddStreamWriter(StreamWriterModel model) {
    _streamWriters.Add(model);

    _logger.LogInformation("Added StreamWriter: SessionId={SessionId}, UserId={UserId}, Topics={Topics}, Filters={Filters}",
      model.SessionId, model.UserId, string.Join(",", model.Topics), string.Join(",", model.Filters));
  }

  // ユーザーがUnListenを読んだときもしくは切断（例外）した時に呼ぶ
  public void RemoveStreamWriter(string sessionId) {
    var writer = _streamWriters.FirstOrDefault(x => x.SessionId == sessionId);
    if (writer != null) {
      _streamWriters.Remove(writer);
    } else {
      _logger.LogWarning("StreamWriter not found for removal: SessionId={SessionId}", sessionId);
    }

    _logger.LogInformation("Removed StreamWriter: SessionId={SessionId}", sessionId);
  }

  public List<StreamWriterModel> GetStreamWriters(Func<StreamWriterModel, bool>? predicate = null) {
    return _streamWriters.Where(x => predicate == null || predicate(x)).ToList();
  }

  public Task WarmUpAsync(IServiceProvider sp, CancellationToken cancellationToken) {
    _logger.LogInformation("StreamHolder service is warming up.");

    // No initialization needed
    return Task.CompletedTask;
  }

  public Task CoolDownAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("StreamHolder service is cooling down.");

    _streamWriters.Clear();

    return Task.CompletedTask;
  }
}
