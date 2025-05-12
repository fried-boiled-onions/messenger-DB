using RabbitMQ.Client;

namespace messengerDB
{
    class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Регистрируем DbService с строкой подключения
                    services.AddScoped<DbService>(sp => 
                        new DbService(hostContext.Configuration.GetConnectionString("DefaultConnection")));

                    services.AddSingleton<IConnection>(sp =>
                    {
                        var config = hostContext.Configuration.GetSection("RabbitMQ");
                        return new ConnectionFactory
                        {
                            HostName = config["HostName"],
                            UserName = config["UserName"],
                            Password = config["Password"]
                        }.CreateConnection();
                    });

                    services.AddSingleton<IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());

                    services.AddSingleton<RabbitMqPublisher>();
                    services.AddSingleton<RabbitMQConsumer>();
                    services.AddHostedService<Worker>();
                })
                .Build()
                .Run();
        }
    }

    public class Worker : BackgroundService
    {
        private readonly RabbitMQConsumer _consumer;
        private readonly RabbitMqPublisher _publisher;

        public Worker(RabbitMQConsumer consumer, RabbitMqPublisher publisher)
        {
            _consumer = consumer;
            _publisher = publisher;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var testMessage = new Request
            {
                Operation = "add_user",
                Data = new Dictionary<string, string>
                {
                    { "username", "testuser" },
                    { "email", "test@example.com" },
                    { "password", "password123" }
                }
            };
            _publisher.PublishMessage("task_queue", testMessage);
            _consumer.StartListening();
            return Task.CompletedTask;
        }
    }
}