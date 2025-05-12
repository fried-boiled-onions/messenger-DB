using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Threading;

namespace messengerDB
{
    class Program
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine("Starting application");
            try
            {
                Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        System.Console.WriteLine("Configuring services");
                        services.AddScoped<DbService>(sp => 
                            new DbService(hostContext.Configuration.GetConnectionString("DefaultConnection")));

                        services.AddSingleton<IConnection>(sp =>
                        {
                            var config = hostContext.Configuration.GetSection("RabbitMQ");
                            System.Console.WriteLine($"Creating RabbitMQ connection: {config["HostName"]}");
                            int retries = 5;
                            int delayMs = 5000; // 5 секунд
                            for (int i = 0; i < retries; i++)
                            {
                                try
                                {
                                    return new ConnectionFactory
                                    {
                                        HostName = config["HostName"],
                                        UserName = config["UserName"],
                                        Password = config["Password"],
                                        Port = 5672
                                    }.CreateConnection();
                                }
                                catch (BrokerUnreachableException ex)
                                {
                                    System.Console.WriteLine($"RabbitMQ connection failed (attempt {i+1}/{retries}): {ex.Message}");
                                    if (i == retries - 1) throw;
                                    Thread.Sleep(delayMs);
                                }
                            }
                            throw new Exception("Failed to connect to RabbitMQ after retries");
                        });

                        services.AddSingleton<IModel>(sp => sp.GetRequiredService<IConnection>().CreateModel());

                        services.AddSingleton<RabbitMqPublisher>(sp =>
                        {
                            var config = hostContext.Configuration.GetSection("RabbitMQ");
                            System.Console.WriteLine("Creating RabbitMqPublisher");
                            return new RabbitMqPublisher(
                                config["HostName"],
                                config["UserName"],
                                config["Password"]
                            );
                        });

                        services.AddSingleton<RabbitMQConsumer>();
                        services.AddHostedService<Worker>();
                        System.Console.WriteLine("Services configured");
                    })
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Application failed: {ex.Message}");
            }
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
            System.Console.WriteLine("Worker initialized");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            System.Console.WriteLine("Worker executing");
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
            System.Console.WriteLine("Worker completed initial tasks");
            return Task.CompletedTask;
        }
    }
}