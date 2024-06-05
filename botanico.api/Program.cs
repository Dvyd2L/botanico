using Grpc.Net.Client;
using GrpcProtos.Protos;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:7219");
Greeter.GreeterClient greeterClient = new(channel);
Esp32.Esp32Client esp32client = new(channel);

Dictionary<String, Delegate> getHandlers = new()
{
  {
    "/",
    async () =>
    {
      // Llama al servicio gRPC
      HelloReply reply = await greeterClient.SayHelloAsync(new HelloRequest { Name = "World" });
      // Devuelve la respuesta del servicio gRPC como respuesta HTTP
      return reply.Message;
    }
  },
  {
      "/{name}",
      async ([FromRoute] String name) =>
      {
          // Llama al servicio gRPC
          HelloReply reply = await greeterClient.SayHelloAsync(new HelloRequest { Name = name });
          // Devuelve la respuesta del servicio gRPC como respuesta HTTP
          return reply.Message;
      }
  },
  {
      "/esp32/toggle-led",
      async () =>
      {
          // Llama al servicio gRPC
          ChangeStatusLedReply reply = await esp32client.ToggleLedAsync(new ChangeStatusLedRequest { Status = false });
          // Devuelve la respuesta del servicio gRPC como respuesta HTTP
          return reply.Status;
      }
  },
  {
      "/esp32/turn-off-led",
      async () =>
      {
          // Llama al servicio gRPC
          ChangeStatusLedReply reply = await esp32client.TurnOffLedAsync(new ChangeStatusLedRequest { Status = false });
          // Devuelve la respuesta del servicio gRPC como respuesta HTTP
          return reply.Status;
      }
  },
  {
      "/esp32/turn-on-led",
      async () =>
      {
          // Llama al servicio gRPC
          ChangeStatusLedReply reply = await esp32client.TurnOnLedAsync(new ChangeStatusLedRequest { Status = true });
          // Devuelve la respuesta del servicio gRPC como respuesta HTTP
          return reply.Status;
      }
  }
};

foreach (KeyValuePair<String, Delegate> entry in getHandlers)
{
  _ = app.MapGet(entry.Key, entry.Value);
}

app.Run();
