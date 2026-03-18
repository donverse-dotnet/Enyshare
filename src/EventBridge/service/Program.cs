using Serilog;
using Pocco.Svc.EventBridge.Services;
using Pocco.Svc.EventBridge.Services.Grpc;

namespace Pocco.Svc.EventBridge;

public class Program {
  public static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog((ctx, cfg) => {
      cfg
        .Enrich.WithThreadId()
        // Set log style -> [yyyy-MM-dd HH:mm:ss.fff] [Level] [SourceContext] Message NewLine Exception
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Level:u4}: {SourceContext}[{ThreadId}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Level:u4}: {SourceContext}[{ThreadId}] {Message:lj}{NewLine}{Exception}")
        .Enrich.FromLogContext();
    });

    // Add services to the container.
    builder.Services.AddSingleton<EventIdProvider>();
    builder.Services.AddSingleton<IHotStartableService>(sp => sp.GetRequiredService<EventIdProvider>());
    builder.Services.AddSingleton<EventSendHelper>();
    builder.Services.AddSingleton<IHotStartableService>(sp => sp.GetRequiredService<EventSendHelper>());
    builder.Services.AddHostedService<HotStarterService>();

    builder.Services.AddGrpc();
    if (builder.Environment.IsDevelopment()) {
      builder.Services.AddGrpcReflection();
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<EventReceiverImpl>();
    app.MapGrpcService<EventDispatcherImpl>();
    if (app.Environment.IsDevelopment()) {
      app.MapGrpcReflectionService();
    }
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
  }
}
