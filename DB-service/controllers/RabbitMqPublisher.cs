using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

public class RabbitMqPublisher
{
    private readonly IModel _channel;

    public RabbitMqPublisher(IModel channel)
    {
        _channel = channel;
    }

    public void PublishMessage<T>(string queueName, T message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }
}