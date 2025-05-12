using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace messengerDB
{
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
            _channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<Request>(message);

                if (request != null)
                {
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
                            break;
                    }
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);
        }
    }
}