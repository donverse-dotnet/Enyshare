using RoleService.Services;
using MongoDB.Driver;
using Pocco.Svc.Roles.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(sp => {
    var connectionString = Environment.GetEnvironmentVariable("TEST_DATABASE_URI") ?? throw new ArgumentException("TEST_DATABASE_URI is not found");

    return new MongoClient(connectionString);
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
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

if (app.Environment.IsDevelopment()) {
    app.MapGrpcReflectionService();
}

app.Run();