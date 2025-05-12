using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Text.Json;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var rabbitMqSection = configuration.GetSection("RabbitMQ");
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        services.AddSingleton(channel);
        services.AddSingleton(new DbService(configuration.GetConnectionString("Postgres")));
        services.AddSingleton<RabbitMqConsumer>();
    });

var consumer = host.Services.GetRequiredService<RabbitMQConsumer>();
consumer.StartListening();

await host.RunAsync();