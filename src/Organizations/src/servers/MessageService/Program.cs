using MessageService.Services;

using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(sp => {
  var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("MongoDb");
  if (string.IsNullOrEmpty(connectionString)) {
    throw new InvalidOperationException("MongoDB connection string is not configured.");
  }

  return new MongoClient(connectionString);
});
builder.Services.AddSingleton<DatabaseManager>();
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrganizationMessageGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
