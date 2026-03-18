using Pocco.Svc.Accounts.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(sp => {
  var connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URI") ?? throw new ArgumentException("TEST_DATABASE_URI is not found");

  return new MongoClient(connectionString);
});


// Add services to the container.
builder.Services.AddGrpc();
if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<UserAccountsService>();
app.MapGrpcService<InternalAccountServiceImpl>();
if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
