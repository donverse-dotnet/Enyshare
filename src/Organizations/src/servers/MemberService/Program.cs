using Grpc.Net.Client;

using MemberService.Repositories;
using MemberService.Services;

using MongoDB.Driver;

using Pocco.Svc.EventBridge.Protobufs.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(sp => {
  var connectionString = Environment.GetEnvironmentVariable("POCCO_DB") ?? throw new
  ArgumentException("POCCO_DB is not set.");
  return new MongoClient(connectionString);
});

builder.Services.AddSingleton<V0EventReceiver.V0EventReceiverClient>(sp => {
  var eventBridgeUrl = Environment.GetEnvironmentVariable("EVENTBRIDGE_URL") ?? throw new ArgumentException("EVENTBRIDGE_URL is not found");
  var channel = GrpcChannel.ForAddress(eventBridgeUrl);
  return new V0EventReceiver.V0EventReceiverClient(channel);
});

builder.Services.AddSingleton<IMemberRepository>(sp => {
  var MongoClient = sp.GetRequiredService<MongoClient>();
  return new MemberRepository(MongoClient);
});

builder.Services.AddSingleton<V0EventReceiver.V0EventReceiverClient>(sp => {
  var eventBridgeUrl = Environment.GetEnvironmentVariable("EVENTBRIDGE_URL") ?? throw new ArgumentException("EVENTBRIDGE_URL is not found");
  var channel = GrpcChannel.ForAddress(eventBridgeUrl);
  return new V0EventReceiver.V0EventReceiverClient(channel);
});

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}

builder.Services.AddGrpc();

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}

//builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrganizationsMemberServiceImpl>();
//app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}

app.Run();
