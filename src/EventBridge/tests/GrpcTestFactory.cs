using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pocco.Svc.EventBridge;
using Pocco.Svc.EventBridge.Services;

namespace Pocco.Svc.EventBridgeTest;

public class GrpcTestFactory : WebApplicationFactory<Program> {
  protected override void ConfigureWebHost(IWebHostBuilder builder) {
    builder.ConfigureTestServices(services => {
      services.AddSingleton(sp => {
        var optionsBuilder = new DbContextOptionsBuilder<V0EventLogStoreService>();
        var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ??
                               throw new InvalidOperationException("MYSQL_CONNECTION_STRING environment variable is not set.");
        optionsBuilder.UseMySQL(connectionString);
        return new V0EventLogStoreService(sp.GetRequiredService<ILogger<V0EventLogStoreService>>(), optionsBuilder.Options);
      });

      services.AddSingleton<V0EventInvoker>();

      services.AddGrpc();
    });

    builder.Configure(app => {
      app.UseRouting();
      app.UseEndpoints(endpoints => {
        endpoints.MapGrpcService<V0AccountEventsImpl>();
        endpoints.MapGrpcService<V0EventSubscriptionServiceImpl>();
      });
    });
  }
}
