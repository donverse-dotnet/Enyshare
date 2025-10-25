using Pocco.Svc.ChatService.Services;
using MongoDB.Driver;
using Pocco.Svc.Chats.Ripositories;
using Pocco.Svc.EventBridge.Protobufs.Services;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(sp => {
  var connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URI") ?? throw new ArgumentException("TEST_DATABASE_URI is not found");
  return new MongoClient(connectionString);
});

builder.Services.AddSingleton<V0EventReceiver.V0EventReceiverClient>(sp => {
  var eventBridgeUrl = Environment.GetEnvironmentVariable("EVENTBRIDGE_URL") ?? throw new ArgumentException("EVENTBRIDGE_URL is not found");
  var channel = GrpcChannel.ForAddress(eventBridgeUrl);
  return new V0EventReceiver.V0EventReceiverClient(channel);
});

builder.Services.AddSingleton<IChatRepository>(sp => {
  var mongoClient = sp.GetRequiredService<IMongoClient>();
  return new ChatRepository(mongoClient);
});

// Add services to the container.
builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrganizationChatService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}

app.Run();