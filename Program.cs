using FileUploadApp.DataAccessLayer;
using FileUploadApp.rabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddScoped<IUploadFileDL, UploadFileDL>();
builder.Services.AddScoped<IFormDL, FormDL>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// app.Run();
Thread thread = new Thread(new ThreadStart(app.Run));
thread.Start();


Thread sub = new Thread(new ThreadStart(Receiver.receiverFunction));
sub.Start();