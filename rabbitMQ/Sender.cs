using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace FileUploadApp.rabbitMQ;

public class Sender
{
    public static void senderFunction(string msg)
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


        channel.QueueDeclare("user", durable: true, exclusive: false, autoDelete: false, arguments: null);

        


        // var jsonString = JsonSerializer.Serialize(msg);
        
        var body = Encoding.UTF8.GetBytes(msg);
        channel.BasicPublish("" , "user" , body : body);
    }
}