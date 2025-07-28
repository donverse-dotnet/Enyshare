using Pocco.Srv.Auth.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IMongoClient>(sp =>
new MongoClient("mongodb://localhost:27017"));

builder.Services.AddSingleton(sp =>
sp.GetRequiredService<IMongoClient>().GetDatabase("MyAppDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
