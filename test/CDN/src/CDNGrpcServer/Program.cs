using Pocco.CDN.Services;

namespace Pocco.CDN;

public class Server {
  public static void Main(string[] args) {
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddGrpc();
    builder.Services.AddGrpcReflection();

    var app = builder.Build();

    app.MapGrpcService<FileStreamService>();
    app.MapGrpcReflectionService();

    app.Run();
  }
}
