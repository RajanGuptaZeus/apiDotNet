using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MySql.Data.MySqlClient;
using FileUploadApp.CommonUtility;

namespace FileUploadApp.rabbitMQ
{
    public class SqlReceiver
    {
        private static readonly string ConnectionString = "server=localhost;user=root;password= ;database=upload_csv;port=3306;";
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
                    using var sqlConnection = new MySqlConnection(ConnectionString);
                    await sqlConnection.OpenAsync();
                    using var sqlCommand = new MySqlCommand(message, sqlConnection);
                    await sqlCommand.ExecuteNonQueryAsync();
                    

                    Console.WriteLine("SQL Command executed successfully: ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in this syntax {message}");
                    Console.WriteLine("Error executing SQL command: " + ex.Message);
                    channel.BasicPublish("" , "sqlCommand" , body : rabbitMQRes);
                }
            };

            channel.BasicConsume("sqlCommand", true, consumer);

            Console.WriteLine("Press any key to stop SQL Receiver");
            Console.ReadKey();
            Console.WriteLine("Stopped SQL Receiver");
        }
    }
}
