using MongoDB.Driver;
using Pocco.Svc.EventBridge.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(sp => {
  var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("MongoDb");
  if (string.IsNullOrEmpty(connectionString)) {
    throw new InvalidOperationException("MongoDB connection string is not configured.");
  }
  return new MongoClient(connectionString);
});

builder.Services.AddSingleton<EventSender>();
builder.Services.AddSingleton<EventDeployInvoker>();
builder.Services.AddSingleton<EventStoreTasksDeployer>();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<EventDispatchGrpcService>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
