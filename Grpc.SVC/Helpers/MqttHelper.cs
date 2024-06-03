using MQTTnet;
using MQTTnet.Client;

namespace Botanico.Grpc.Helpers;

public interface IMqttHelper
{
    Task<MqttClientConnectResult> ConnectAsync();
}

public class MqttHelper(MqttFactory factory, IConfiguration config) : IMqttHelper
{
    private readonly IMqttClient _client = factory.CreateMqttClient();
    private readonly IConfiguration _config = config;

    public async Task<MqttClientConnectResult> ConnectAsync()
    {
        MqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId(this._config["HiveMQ:CilentID"])
            .WithTcpServer(this._config["HiveMQ:ClusterUrl"], Int32.Parse(this._config["HiveMQ:Port"] ?? ""))
            .WithCredentials(this._config["HiveMQ:Username"], this._config["HiveMQ:Password"])
            .WithCleanSession()
            .Build();
        return await this._client.ConnectAsync(options, CancellationToken.None);
    }
}
