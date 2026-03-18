using Grpc.Net.Client;

using MessageService.Services;

using MongoDB.Driver;

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
  var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("MongoDb");
  if (string.IsNullOrEmpty(connectionString)) {
    throw new InvalidOperationException("MongoDB connection string is not configured.");
  }

  return new MongoClient(connectionString);
});

builder.Services.AddSingleton<V0EventReceiver.V0EventReceiverClient>(sp => {
  var eventBridgeUrl = Environment.GetEnvironmentVariable("EVENTBRIDGE_URL") ?? throw new ArgumentException("EVENTBRIDGE_URL is not found");
  var channel = GrpcChannel.ForAddress(eventBridgeUrl);
  return new V0EventReceiver.V0EventReceiverClient(channel);
});

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}
builder.Services.AddSingleton<DatabaseManager>();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrganizationMessageGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}
app.Run();
