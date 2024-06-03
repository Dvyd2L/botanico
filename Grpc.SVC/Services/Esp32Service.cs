using Grpc.Core;
using GrpcProtos.Protos;
namespace botanico.grpc.Services;

public class Esp32Service(ILogger<Esp32Service> logger) : Esp32.Esp32Base
{
    private readonly ILogger<Esp32Service> _logger = logger;

    public override Task<ChangeStatusLedReply> ToggleLed(ChangeStatusLedRequest request, ServerCallContext context)
    {
        this._logger.Log(LogLevel.Debug, "Received request: {@Request}", request);

        return Task.FromResult(new ChangeStatusLedReply
        {
            Status = !request.Status
        });
    }

    public override Task<ChangeStatusLedReply> TurnOffLed(ChangeStatusLedRequest request, ServerCallContext context)
    {
        this._logger.Log(LogLevel.Debug, "Received request: {@Request}", request);

        return Task.FromResult(new ChangeStatusLedReply
        {
            Status = false,
        });
    }

    public override Task<ChangeStatusLedReply> TurnOnLed(ChangeStatusLedRequest request, ServerCallContext context)
    {
        this._logger.Log(LogLevel.Debug, "Received request: {@Request}", request);

        return Task.FromResult(new ChangeStatusLedReply
        {
            Status = true,
        });
    }
}
