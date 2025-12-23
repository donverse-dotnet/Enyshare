using RoleService.Services;
using MongoDB.Driver;
using Pocco.Svc.Roles.Repositories;
using Grpc.Net.Client;
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

builder.Services.AddSingleton<IMongoClient>(sp => {
  var connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URI") ?? throw new ArgumentException("TEST_DATABASE_URI is not found");

  return new MongoClient(connectionString);
});

builder.Services.AddSingleton<V0EventReceiver.V0EventReceiverClient>(sp => {
  var eventBridgeUrl = Environment.GetEnvironmentVariable("EVENTBRIDGE_URL") ?? throw new ArgumentException("EVENTBRIDGE_URL is not found");
  var channel = GrpcChannel.ForAddress(eventBridgeUrl);
  return new V0EventReceiver.V0EventReceiverClient(channel);
});

builder.Services.AddSingleton<IRoleRepository>(sp => {
  var mongoClient = sp.GetRequiredService<IMongoClient>();

  return new RoleRepository(mongoClient);
});

// Add services to the container.
builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrganizationRoleService>();
app.MapGrpcService<V0InternalRoleServiceImpl>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}

app.Run();
