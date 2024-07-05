using RabbitMQ.Client;
using Polly;
using Polly.Retry;
using System;
using RabbitMQ.Client.Exceptions;

public class RabbitMQConnection
{
    private static ConnectionFactory _factory;

    static RabbitMQConnection()
    {
        _factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "root",
            Password = "root123",
            VirtualHost = "/"
        };
    }

    public static IModel? ConnectToRabbit()
    {
        try
        {  
            RetryPolicy retryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetry(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, timespan) =>
                    {
                        Console.WriteLine($"Retry due to {ex}. Waiting for {timespan} before next retry.");
                    }
                );

            return retryPolicy.Execute(() =>
            {
                var conn = _factory.CreateConnection();
                var channel = conn.CreateModel();
                return channel;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to RabbitMQ: {ex}");
            return null;
        }
    }
}
