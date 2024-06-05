using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace Botanico.Grpc.Helpers;

/// <summary>
/// Enumeración de los distintos tipos de Calidad de Sevicio (QoS) del protocolo MQTT.
/// </summary>
public enum MqttQoSLevel
{
    /// <summary>
    /// El mensaje se entrega a lo sumo una vez, o puede no entregarse en absoluto. No hay confirmación.
    /// </summary>
    AtMostOnce = 0,

    /// <summary>
    /// El mensaje se entrega al menos una vez, pero puede entregarse más de una vez en caso de reintentos.
    /// </summary>
    AtLeastOnce = 1,

    /// <summary>
    /// El mensaje se entrega exactamente una vez mediante un mecanismo de handshake de cuatro pasos.
    /// </summary>
    ExactlyOnce = 2,
}

public interface IMqttService
{
    /// <summary>
    /// Propiedad para saber el estado de conexión con el broker MQTT.
    /// </summary>
    Boolean IsConnected { get; }

    /// <summary>
    /// Establece una conexión con el bróker MQTT utilizando las opciones configuradas.
    /// </summary>
    /// <returns>Una tarea que representa la operación de conexión asíncrona. La tarea
    /// contiene el resultado de la conexión.</returns>
    Task<MqttClientConnectResult?> ConnectAsync();

    /// <summary>
    /// Publica un mensaje en un topic específico.
    /// </summary>
    /// <param name="topic">El topic al que se enviará el mensaje.</param>
    /// <param name="payload">El contenido del mensaje.</param>
    /// <param name="qos">El nivel de calidad de servicio (QoS) para la publicación.</param>
    /// <param name="retain">Indica si el mensaje debe ser retenido por el bróker.</param>
    /// <returns>Una tarea que representa la operación de publicación asíncrona. La tarea
    /// contiene el resultado de la publicación.</returns>
    Task<MqttClientPublishResult> PublishAsync(String topic, String payload, MqttQoSLevel qos = MqttQoSLevel.AtMostOnce, Boolean retain = false);

    /// <summary>
    /// Se suscribe a un topic específico para recibir mensajes.
    /// </summary>
    /// <param name="topic">El topic al que se desea suscribir.</param>
    /// <param name="qos">El nivel de calidad de servicio (QoS) para la suscripción.</param>
    /// <returns>Una tarea que representa la operación de suscripción asíncrona. La tarea
    /// contiene el resultado de la suscripción.</returns>
    Task<MqttClientSubscribeResult> SubscribeAsync(String topic, MqttQoSLevel qos = MqttQoSLevel.AtMostOnce);
}

public class MqttService(MqttFactory factory, IConfiguration config) : IMqttService
{
    private readonly IMqttClient _client = factory.CreateMqttClient();
    private readonly IConfiguration _config = config;
    private MqttClientConnectResult? _lastConnectResult = null; // Almacena el último resultado de conexión
    public Boolean IsConnected { get => this._lastConnectResult?.IsSessionPresent ?? false; }

    public async Task<MqttClientConnectResult?> ConnectAsync()
    {
        if (!this.IsConnected)
        {
            MqttClientOptions options = new MqttClientOptionsBuilder()
                .WithClientId(this._config["HiveMQ:ClientID"])
                .WithTcpServer(this._config["HiveMQ:ClusterUrl"], Int32.Parse(this._config["HiveMQ:Port"] ?? ""))
                .WithCredentials(this._config["HiveMQ:Username"], this._config["HiveMQ:Password"])
                .WithTlsOptions(new MqttClientTlsOptions
                {
                    UseTls = true,
                    AllowUntrustedCertificates = false,
                    IgnoreCertificateChainErrors = false,
                    IgnoreCertificateRevocationErrors = false
                })
                .WithCleanSession()
                .Build();

            this._lastConnectResult = await this._client.ConnectAsync(options, CancellationToken.None);
        }

        return this._lastConnectResult;
    }

    public async Task<MqttClientPublishResult> PublishAsync(String topic, String payload, MqttQoSLevel qos = MqttQoSLevel.AtMostOnce, Boolean retain = false)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
            .WithRetainFlag(retain)
            .Build();

        return await this._client.PublishAsync(message, CancellationToken.None);
    }

    public async Task<MqttClientSubscribeResult> SubscribeAsync(String topic, MqttQoSLevel qos = MqttQoSLevel.AtMostOnce)
    {
        MQTTnet.Packets.MqttTopicFilter topicFilter = new MqttTopicFilterBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
            .Build();

        return await this._client.SubscribeAsync(topicFilter, CancellationToken.None);
    }
}
