using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace FileUploadApp.CommonUtility
{
    class MongooseConnect
    {
        public static string mongoClient = "mongodb://localhost:27017";

        public static IMongoDatabase? mongoConnection()
        {
            RetryPolicy retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(
                retryCount: 3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, timespan) =>
                {
                    Console.WriteLine($"Retry attempt {timespan.TotalSeconds} seconds later because of {ex.Message} .");
                }
            );

            return retryPolicy.Execute(() =>
            {
                try
                {
                    var client = new MongoClient(mongoClient);
                    var database = client.GetDatabase("file_upload");
                    // var collection = database.GetCollection<LogEntry>("log");

                    Console.WriteLine("Connected to MongoDB");
                    return database;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while connecting or inserting data: " + ex);
                    return null;
                }
            });
        }
    }
}
