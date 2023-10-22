using Application.Services;
using Infrastucture.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//Injecting shared dependencies
builder.Services.AddTransient<ITorrentService, TorrentService>();
builder.Services.AddTransient<ITorrentNotifier, TorrentNotifier>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var torrentService = app.Services.GetService<ITorrentService>();
var cta = new CancellationToken();
await torrentService!.StartServer(cta);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
