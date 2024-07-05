using System.Data;
using MySql.Data.MySqlClient;
using Polly;
using Polly.Retry;

namespace FileUploadApp.CommonUtility;



public class SQLConnection
{
    private static readonly string ConnectionString = "server=localhost;user=root;password= ;database=upload_csv;port=3306;";
    private static MySqlConnection? sqlConnection;
    public static async Task<MySqlConnection?> ConnectToDb()
    {
        AsyncRetryPolicy retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, timespan) =>
                {
                    Console.WriteLine($"Retry attempt {timespan.TotalSeconds} seconds later because of {ex}");
                }
            );

        try
        {
            return await retryPolicy.ExecuteAsync(async () =>
            {
                sqlConnection = new MySqlConnection(ConnectionString);
                if (sqlConnection.State != ConnectionState.Open)
                {
                    await sqlConnection.OpenAsync();
                    Console.WriteLine("Connected To database");
                }
                return sqlConnection;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred during database connection: {ex}");
            return null;
        }
    }
}