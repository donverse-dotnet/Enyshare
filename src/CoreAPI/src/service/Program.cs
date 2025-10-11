using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Pocco.Libs.Protobufs.Services;
using Pocco.Svc.CoreAPI.Auth;
using Pocco.Svc.CoreAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Auth handlers
builder.Services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, GeneralAuthorizationHandler>();
// gRPC services
builder.Services.AddGrpc();
if (builder.Environment.IsDevelopment()) {
  builder.Services.AddGrpcReflection();
}

builder.Services.AddTransient(sp => {
  var serverAddress = Environment.GetEnvironmentVariable("AUTH_SERVICE_ADDRESS") ?? throw new InvalidOperationException("AUTH_SERVICE_ADDRESS environment variable is not set.");

  var channel = GrpcChannel.ForAddress(serverAddress);
  return new V0AuthService.V0AuthServiceClient(channel);
});

builder.Services.AddAuthentication()
  .AddScheme<AuthenticationSchemeOptions, AuthenticateHandler>("BaseAuth", options => { });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy => {
      policy.Requirements.Add(new AuthorizationRequirement());
      policy.RequireRole("Admin");
    })
    .AddPolicy("RequireGeneral", policy => {
      policy.Requirements.Add(new AuthorizationRequirement());
      policy.RequireRole("User", "Admin");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
// Add GrpcServices to here.
if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}
app.MapGrpcService<AccountsServiceImpl>();
app.MapGrpcService<EventsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Run app
app.Run();
