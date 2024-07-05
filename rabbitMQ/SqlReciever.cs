using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MySql.Data.MySqlClient;
using FileUploadApp.CommonUtility;

namespace FileUploadApp.rabbitMQ
{
    public class SqlReceiver
    {
        
        private static byte[]? rabbitMQRes;

        public static void SqlReceiverFunction()
        {
            Console.WriteLine("Started SQL Receiver");

            var channel = RabbitMQConnection.ConnectToRabbit();

            channel.QueueDeclare("sqlCommand", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                rabbitMQRes = body;
                var message = Encoding.UTF8.GetString(body);

                // Console.WriteLine(message);
                try
                {
                    var sqlCommand = await SQLConnection.ConnectToDb(message);
                    await sqlCommand.ExecuteNonQueryAsync();

                    Console.WriteLine("SQL Command executed successfully: ");
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"Error in this syntax {message}");
                    Console.WriteLine("Error executing SQL command: " + ex.Message);
                    channel.BasicPublish("", "sqlCommand", body: rabbitMQRes);
                }
            };

            channel.BasicConsume("sqlCommand", true, consumer);
            
            Console.WriteLine("Press any key to stop SQL Receiver");
            Console.ReadKey();
            Console.WriteLine("Stopped SQL Receiver");
        }

        private static void ListFiles(string rootFolder)
        {
            try
            {
                // Process all files in the root folder and its subfolders recursively
                foreach (string file in Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories))
                {
                    Console.WriteLine(rootFolder);
                    Console.WriteLine(file);
                    System.IO.File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
