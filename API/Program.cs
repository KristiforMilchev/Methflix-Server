using Application.Services;
using Domain.Context;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;
// Add services to the container.
 
builder.Services.AddControllers();
builder.Services.AddDbContext<MethflixContext>(
    options =>
        options.UseNpgsql(configuration.GetConnectionString("PostgradeSQL"))
);

//Injecting shared dependencies
var notifier = new TorrentNotifier();
builder.Services.AddSingleton<ITorrentNotifier>(notifier);
builder.Services.AddSingleton<ITorrentService>(new TorrentService(configuration, notifier));
builder.Services.AddTransient<IFfmpegService, FfmpegService>();
builder.Services.AddTransient<IStorageService, StorageService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var torrentService = app.Services.GetService<ITorrentService>();
var cta = new CancellationToken();
Task.Run((async () => await torrentService!.StartServer(cta)));
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
