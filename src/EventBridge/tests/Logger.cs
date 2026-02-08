using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Pocco.Svc.EventBridgeTest;

public class Logger : ILogger {
  private readonly ITestOutputHelper _outputHelper;
  private readonly string _categoryName;

  public Logger(ITestOutputHelper outputHelper, string categoryName) {
    _outputHelper = outputHelper;
    _categoryName = categoryName.Split('.').Last();
  }

  public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
    return NoopDisposable.Instance;
  }

  public bool IsEnabled(LogLevel logLevel) {
    return true; // Always enabled for test output
  }

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
    if (!IsEnabled(logLevel)) {
      return;
    }

    if (exception is not null) {
      _outputHelper.WriteLine($"[{logLevel}] {_categoryName}: {formatter(state, exception)}");
      _outputHelper.WriteLine(exception.ToString());
      return;
    } else {
      _outputHelper.WriteLine($"[{logLevel}] {_categoryName}: {formatter(state, null)}");
    }
  }

  private class NoopDisposable : IDisposable {
    public static NoopDisposable Instance = new NoopDisposable();
    public void Dispose() { }
  }
}
