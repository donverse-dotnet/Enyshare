using Microsoft.EntityFrameworkCore;
using Pocco.Svc.EventBridge.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(sp => {
  var optionsBuilder = new DbContextOptionsBuilder<V0EventLogStoreService>();
  var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ??
                         throw new InvalidOperationException("MYSQL_CONNECTION_STRING environment variable is not set.");
  optionsBuilder.UseMySQL(connectionString);
  return new V0EventLogStoreService(sp.GetRequiredService<ILogger<V0EventLogStoreService>>(), optionsBuilder.Options);
});

builder.Services.AddSingleton<V0EventInvoker>();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<V0AccountEventsImpl>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
