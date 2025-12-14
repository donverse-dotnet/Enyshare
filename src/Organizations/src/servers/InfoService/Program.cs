using Grpc.Net.Client;

using InfoService.Services;

using MongoDB.Driver;

using Pocco.Libs.Protobufs.Accounts.Services;
using Pocco.Libs.Protobufs.EventBridge.Services;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog((ctx, cfg) => {
  cfg
    .Enrich.WithThreadId()
    // Set log style -> [yyyy-MM-dd HH:mm:ss.fff] [Level] [SourceContext][[ThreadId]] Message NewLine Exception
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Level:u4}: {SourceContext}[{ThreadId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Level:u4}: {SourceContext}[{ThreadId}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext();
});

// Add services to the container.
builder.Services.AddSingleton(sp => {
  var connectionString = Environment.GetEnvironmentVariable("POCCO_DB") ?? throw new ArgumentException("POCCO_DB is not set.");
  var client = new MongoClient(connectionString);
  return client.GetDatabase("Entities");
});

builder.Services.AddSingleton(sp => {
  var eventBridgeUrl = Environment.GetEnvironmentVariable("EVENTBRIDGE_URL") ?? throw new ArgumentException("EVENTBRIDGE_URL is not found");
  var channel = GrpcChannel.ForAddress(eventBridgeUrl);
  return new V0EventReceiver.V0EventReceiverClient(channel);
});

builder.Services.AddSingleton(sp => {
  var internalAccountSvc = Environment.GetEnvironmentVariable("INTERNAL_ACCOUNT_SVC_URL") ?? throw new ArgumentException("INTERNAL_ACCOUNT_SVC_URL is not found");
  Console.WriteLine($"InternalAccountService URL: {internalAccountSvc}");
  var channel = GrpcChannel.ForAddress(internalAccountSvc);
  return new V0InternalAccountService.V0InternalAccountServiceClient(channel);
});

builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrganizationsInfoServiceImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}

app.Run();
