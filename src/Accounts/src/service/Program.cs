using Pocco.Svc.Accounts.Services;
using MongoDB.Driver;
using Pocco.Svc.Accounts.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Pocco.Svc.Accounts.Settings.MongoDatabaseSettings>(
    builder.Configuration.GetSection("MongoDatabaseSettings"));

builder.Services.AddSingleton<IMongoClient>(sp => {
    var settings = sp.GetRequiredService<IOptions<Pocco.Svc.Accounts.Settings.MongoDatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp => {
    var settings = sp.GetRequiredService<IOptions<Pocco.Svc.Accounts.Settings.MongoDatabaseSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});


// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<UserAccountsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
