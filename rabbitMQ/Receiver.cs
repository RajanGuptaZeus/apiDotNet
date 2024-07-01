using System.Text;
using FileUploadApp.DataAccessLayer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileUploadApp.rabbitMQ;


public class Receiver
{
    // public readonly IUploadFileDL _uploadFileDL;



    // public Receiver(IUploadFileDL uploadFileDL)
    // {
    //     // dependency injection implement kiya - DI
    //     _uploadFileDL = uploadFileDL;
    // }
    // public Receiver()
    // {
    //     // dependency injection implement kiya - DI
    // }
    public static void receiverFunction()
    {
        Console.WriteLine("Started receiver");
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "root",
            Password = "root123",
            VirtualHost = "/"
        };
        var conn = factory.CreateConnection();

        using var channel = conn.CreateModel();

        channel.QueueDeclare("user", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            // await _uploadFileDL.UploadCsvFile(message);
            var res = await UploadFileDL.UploadCsvFile(message);
            Console.WriteLine(res.Message);
            Console.WriteLine(res.IsSuccess);
        };

        channel.BasicConsume("user", true, consumer);
        Console.ReadKey();
        Console.WriteLine("Stopped Receiver");
        // return message;
    }
}