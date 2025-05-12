using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMQConsumer
{
    private readonly IModel _channel;
    private readonly DbService _dbService;

    public RabbitMQConsumer(IModel channel, DbService dbService)
    {
        _channel = channel;
        _dbService = dbService;
    }

    public void StartListening()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var request = JsonSerializer.Deserialize<Request>(message);

            if (request == null)
            {
                Console.WriteLine("Failed to deserialize message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
                return;
            }

            switch (request.Operation)
            {
                case "add_user":
                    await _dbService.AddUserAsync(
                        request.Data["username"],
                        request.Data["email"],
                        request.Data["password"]
                    );
                    break;

                case "send_message":
                    await _dbService.SendMessageAsync(
                        int.Parse(request.Data["chat_id"]),
                        int.Parse(request.Data["sender_id"]),
                        request.Data["content"]
                    );
                    break;

                case "get_new_chats":
                    var chats = await _dbService.GetNewChatsAsync(int.Parse(request.Data["user_id"]));
                    Console.WriteLine($"Found {chats.Count()} chats");
                    break;

                default:
                    Console.WriteLine("Unknown operation");
                    break;
            }
        };

        _channel.BasicConsume(queue: "task_queue", autoAck: true, consumer: consumer);
    }
}