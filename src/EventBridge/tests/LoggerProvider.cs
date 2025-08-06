using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pocco.Svc.EventBridgeTest;

public class LoggerProvider : ILoggerProvider {
  private readonly ITestOutputHelper _outputHelper;

  public LoggerProvider(ITestOutputHelper outputHelper) {
    _outputHelper = outputHelper;
  }

  public ILogger CreateLogger(string categoryName) {
    return new Logger(_outputHelper, categoryName);
  }

  public void Dispose() {
    // No resources to dispose
  }
}
