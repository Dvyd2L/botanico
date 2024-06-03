using Grpc.Core;
using GrpcProtos.Protos;

namespace Botanico.Grpc.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger = logger;

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        this._logger.Log(LogLevel.Debug, "Received request: {@Request}", request);

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
