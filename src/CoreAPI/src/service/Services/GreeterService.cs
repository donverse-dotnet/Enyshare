using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Pocco.Svc.CoreAPI;

namespace Pocco.Svc.CoreAPI.Services;

public class GreeterService : Greeter.GreeterBase {
  private readonly ILogger<GreeterService> _logger;
  public GreeterService(ILogger<GreeterService> logger) {
    _logger = logger;
  }

  [Authorize(Policy = "Authenticated")]
  public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context) {
    return Task.FromResult(new HelloReply {
      Message = "Hello " + request.Name
    });
  }
}
