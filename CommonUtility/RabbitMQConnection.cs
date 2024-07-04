using RabbitMQ.Client;

public class RabbitMQConnection
{
    public static IModel ConnectToRabbit()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "root",
            Password = "root123",
            VirtualHost = "/"
        };
        var conn = factory.CreateConnection();
        var channel = conn.CreateModel();

        return channel;
    }
}
