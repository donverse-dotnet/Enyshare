namespace Pocco.Svc.EventBridge.Services;

public class HotStarterService : IHostedService {
  // Kestrel server items
  private readonly IServiceProvider _serviceProvider;
  private readonly IHostApplicationLifetime _lifetime;
  private readonly ILogger<HotStarterService> _logger;

  public HotStarterService(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime,
    ILogger<HotStarterService> logger) {
    _serviceProvider = serviceProvider;
    _lifetime = lifetime;
    _logger = logger;

    _logger.LogInformation("HotStarterService initialized");
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("HotStarterService starting...");
    // Perform startup tasks here
    _lifetime.ApplicationStarted.Register(async () => {
      _logger.LogInformation("Application has started.");

      // Get all services that implement IHotStartableService and call their StartAsync methods
      using var scope = _serviceProvider.CreateScope();
      var hotStartableServices = scope.ServiceProvider.GetServices<IHotStartableService>().ToArray();
      var startTasks = hotStartableServices.Select(service => SafeStartServiceAsync(service, scope.ServiceProvider, cancellationToken)).ToArray();
      await Task.WhenAll(startTasks);

      _logger.LogInformation("All hot startable services have been started.");
    });

    await Task.CompletedTask;
  }

  private async Task SafeStartServiceAsync(IHotStartableService service, IServiceProvider sp, CancellationToken cancellationToken) {
    try {
      await service.WarmUpAsync(sp, cancellationToken);
      _logger.LogInformation("Service {ServiceName} started successfully.", service.GetType().Name);
    } catch (Exception ex) {
      _logger.LogError(ex, "Error starting service {ServiceName}.", service.GetType().Name);
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken) {
    _logger.LogInformation("HotStarterService stopping...");

    // Register application stopping event
    _lifetime.ApplicationStopped.Register(() => {
      Task.Run(async () => {
        try {
          await SafeStopServicesAsync();
        } catch (Exception ex) {
          _logger.LogError(ex, "Unhandled exception during application stop.");
        }
      });
    });

    await Task.CompletedTask;
  }

  private async Task SafeStopServicesAsync()
  {
    _logger.LogInformation("Application is stopping...");

    using var scope = _serviceProvider.CreateScope();
    var hotStartableServices = scope.ServiceProvider.GetServices<IHotStartableService>().ToArray();
    foreach (var service in hotStartableServices)
    {
      try
      {
        await service.CoolDownAsync(CancellationToken.None);
        _logger.LogInformation("Service {ServiceName} stopped successfully.", service.GetType().Name);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error stopping service {ServiceName}.", service.GetType().Name);
      }
    }

    _logger.LogInformation("All hot startable services have been stopped.");
  }
}
