using System;
using System.Threading.Tasks;
using FileUploadApp.CommonLayer.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace FileUploadApp.CommonUtility
{
    public class MongooseConnect
    {
        private static readonly string mongoClient = "mongodb://localhost:27017";
        private static IMongoCollection<LogEntry> collection = null;

        public static IMongoCollection<LogEntry> Connect()
        {
            if (collection != null)
                return collection;

            RetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    retryCount: 3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, timespan) =>
                    {
                        Console.WriteLine($"Retry attempt {timespan.TotalSeconds} seconds later because of {ex.Message}.");
                    }
                );

            retryPolicy.Execute(() =>
            {
                try
                {
                    var client = new MongoClient(mongoClient);
                    var database = client.GetDatabase("file_upload");
                    collection = database.GetCollection<LogEntry>("log");

                    Console.WriteLine("Connected to MongoDB");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while connecting or inserting data: " + ex.Message);
                    throw;
                }
            });

            return collection;
        }

        public static async Task<bool> UpdateDocumentById(string id, string updatedFieldName, string updatedValue)
        {
            try
            {
                var objectId = ObjectId.Parse(id);

                var filter = Builders<LogEntry>.Filter.Eq("_id", objectId);
                var update = Builders<LogEntry>.Update.Set(updatedFieldName, updatedValue);

                var result = await Connect().UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
