using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MySql.Data.MySqlClient;
using FileUploadApp.CommonUtility;
using System.IO;

namespace FileUploadApp.rabbitMQ
{
    public class SqlReceiver
    {
        private static byte[]? rabbitMQRes;
        private static MySqlConnection? sqlConnectionToDB;
        private static IModel? channel;

        public static async void SqlReceiverFunction()
        {
            Console.WriteLine("Started SQL Receiver");

            channel = RabbitMQConnection.ConnectToRabbit();

            channel.QueueDeclare("sqlCommand", durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            sqlConnectionToDB = await SQLConnection.ConnectToDb();
            int i = 0;

            consumer.Received += async (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                rabbitMQRes = body;
                var msg = Encoding.UTF8.GetString(body);
                var id = msg.Split("|")[0];
                var status = MongooseConnect.UpdateDocumentById(id,"Message",$"Executing batch no : {i+1}");
                var message = msg.Split("|")[1];
                try
                {
                    var sqlCommand = new MySqlCommand(message, sqlConnectionToDB);
                    await sqlCommand.ExecuteNonQueryAsync();
                    i++;
                    // Console.WriteLine(i);
                    Console.WriteLine("SQL Command executed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing SQL command: " + ex.Message);
                    channel.BasicPublish("", "sqlCommand", body: rabbitMQRes);
                } 
            };

            channel.BasicConsume("sqlCommand", true, consumer);

            Console.WriteLine("Press any key to stop SQL Receiver");
            Console.ReadKey();
            Console.WriteLine("Stopped SQL Receiver");
        }

        private static void DisposeConnections()
        {
            try
            {
                if (sqlConnectionToDB != null)
                {
                    sqlConnectionToDB.Close();
                    sqlConnectionToDB.Dispose();
                    Console.WriteLine("SQL Connection disposed");
                }

                if (channel != null)
                {
                    channel.Close();
                    channel.Dispose();
                    Console.WriteLine("RabbitMQ Channel disposed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing connections: {ex.Message}");
                throw new Exception (ex.Message);              
            }
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
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
