using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Pocco.Libs.Protobufs.Services;
using Pocco.Svc.CoreAPI.Handlers;
using Pocco.Svc.CoreAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddTransient(sp => {
  var serverAddress = Environment.GetEnvironmentVariable("AUTH_SERVICE_ADDRESS") ?? throw new InvalidOperationException("AUTH_SERVICE_ADDRESS environment variable is not set.");

  var channel = GrpcChannel.ForAddress(serverAddress);
  return new V0AuthService.V0AuthServiceClient(channel);
});

builder.Services.AddAuthentication("AuthService")
    .AddScheme<AuthenticationSchemeOptions, AuthHandler>("AuthService", options => { });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
