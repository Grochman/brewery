using System.Text;
using MQTTnet;
using MQTTnet.Client;


namespace Brewery;

public class MqttService: BackgroundService
{
    private IMqttClient? _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 9001)
            .Build();

        await _client.ConnectAsync(options, stoppingToken);

        Console.WriteLine("MQTT connected.");

        _client.ApplicationMessageReceivedAsync += e =>
        {
            Console.WriteLine("Message: " + e.ApplicationMessage.ConvertPayloadToString());
            return Task.CompletedTask;
        };

        await _client.SubscribeAsync(
            new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("TEMPERATURE")
                .WithTopicFilter("PRESSURE")
                .WithTopicFilter("CO2")
                .WithTopicFilter("OXYGEN")
                .Build());

        
        _client.ApplicationMessageReceivedAsync += e =>
        {
            switch  (e.ApplicationMessage.Topic)
            {
                case "TEMPERATURE":
                    Console.WriteLine($"Temperature message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    break;
                case "PRESSURE":
                    Console.WriteLine($"Pressure message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    break;
                case "CO2":
                    Console.WriteLine($"CO2 message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    break;
                case "OXYGEN":
                    Console.WriteLine($"Oxygen message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    break;
            }
            return Task.CompletedTask;
        };
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}