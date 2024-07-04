using System.Text;
using System.Text.Json;
using FileUploadApp.CommonUtility;
using RabbitMQ.Client;

namespace FileUploadApp.rabbitMQ;

public class Sender
{
    public static void senderFunction(string queueName,string msg)
    {
        var channel = RabbitMQConnection.ConnectToRabbit();


        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        


        // var jsonString = JsonSerializer.Serialize(msg);
        
        var body = Encoding.UTF8.GetBytes(msg);
        channel.BasicPublish("" , queueName , body : body);
    }
}