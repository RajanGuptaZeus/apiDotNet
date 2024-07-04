using System.Text;
using FileUploadApp.CommonUtility;
using FileUploadApp.DataAccessLayer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Receiver
{
    public static void receiverFunction()
    {
        Console.WriteLine("Started receiver");

        var channel = RabbitMQConnection.ConnectToRabbit();

        try
        {
            channel.QueueDeclare("user", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var res = await UploadFileDL.UploadCsvFile(message);
                Console.WriteLine(res.Message);
                Console.WriteLine(res.IsSuccess);
            };

            channel.BasicConsume("user", true, consumer);
            Console.ReadKey();
        }
        finally
        {
            // Ensure to properly close the channel and connection
            channel.Close();  // Close the channel
            channel.Dispose();  // Dispose the channel
        }

        Console.WriteLine("Stopped Receiver");
    }
}
