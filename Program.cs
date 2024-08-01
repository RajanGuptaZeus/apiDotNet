using FileUploadApp.rabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace FileUploadApp
{
    public class Program
    {
        private const string MyAllowSpecificOrigins = "";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register services
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://127.0.0.1:5500")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            // Build the app
            var app = builder.Build();

            // Configure the app
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors(MyAllowSpecificOrigins);
            app.MapControllers();

            // Start background threads
            Thread receiverThread = new Thread(Receiver.receiverFunction);
            receiverThread.Start();

            Thread sqlReceiverThread = new Thread(SqlReceiver.SqlReceiverFunction);
            sqlReceiverThread.Start();

            app.Run();
        }
    }
}
