using API;
using Application.Repositories;
using Application.Services;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;
// Add services to the container.
 
builder.Services.AddControllers();
builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(configuration.GetConnectionString("YourConnectionString")));

//Injecting shared dependencies
var notifier = new TorrentNotifier();
builder.Services.AddSingleton<ITorrentNotifier>(notifier);
builder.Services.AddSingleton<ITorrentService>(new TorrentService(configuration, notifier));
builder.Services.AddTransient<IFfmpegService, FfmpegService>();
builder.Services.AddTransient<IStorageService, StorageService>();

//Repositories
builder.Services.AddTransient<ITvShowsRepository, TvShowsRepository>();
builder.Services.AddTransient<IMovieRepository, MovieRepository>();
builder.Services.AddTransient<ITorrentRepository, TorrentRepository>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<ICdnService, CdnService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var scope = app.Services.CreateScope();
var torrentService = app.Services.GetService<ITorrentService>();
var torrentRepository = scope.ServiceProvider.GetService<ITorrentRepository>();

using var common = new Common(torrentRepository!);

var cta = new CancellationToken();
Task.Run((async () => await torrentService!.StartServer(cta)));
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();

app.MapControllers();

app.Run();
