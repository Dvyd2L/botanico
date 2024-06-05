using Botanico.Grpc.Helpers;
using Grpc.Core;
using GrpcProtos.Protos;

namespace Botanico.Grpc.Services;

public class Esp32Service(ILogger<Esp32Service> logger, IMqttService mqtt) : Esp32.Esp32Base
{
    private readonly ILogger<Esp32Service> _logger = logger;
    private readonly IMqttService _mqtt = mqtt;
    private const String _TOPIC = "esp32/gpio2";

    public override async Task<ChangeStatusLedReply> ToggleLed(ChangeStatusLedRequest request, ServerCallContext context) =>
        await this.HandleLedChange(request, payload: request.Status ? "off" : "on", !request.Status);

    public override async Task<ChangeStatusLedReply> TurnOffLed(ChangeStatusLedRequest request, ServerCallContext context) =>
        await this.HandleLedChange(request, payload: "off", false);

    public override async Task<ChangeStatusLedReply> TurnOnLed(ChangeStatusLedRequest request, ServerCallContext context) =>
        await this.HandleLedChange(request, payload: "on", true);

    private async Task<ChangeStatusLedReply> HandleLedChange(ChangeStatusLedRequest request, String payload, Boolean newStatus)
    {
        this._logger.Log(LogLevel.Debug, "Received request: {@Request}", request);

        if (!this._mqtt.IsConnected)
        {
            _ = await this._mqtt.ConnectAsync();
        }

        _ = await this._mqtt.PublishAsync(_TOPIC, payload);

        return new ChangeStatusLedReply { Status = newStatus };
    }
}
