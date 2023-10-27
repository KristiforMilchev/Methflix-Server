using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class StorageService : IStorageService
{
    private readonly string _movieDirectory;
    
    public StorageService(IConfiguration configuration)
    {
        _movieDirectory = configuration["StorageManager:Internal"] ?? string.Empty;
    }
    
    public string GetFilePath(string movie)
    {
        return !File.Exists($"{_movieDirectory}/{movie}") ? string.Empty : $"{_movieDirectory}/{movie}";
    }

    public string GetFileName(string movie)
    {
        movie = Path.GetFileName(movie);
        return Path.GetFileNameWithoutExtension(movie)
               + "_"
               + Guid.NewGuid().ToString().Substring(0, 6)
               + Path.GetExtension(movie);
    }

    public string GetFileExtension(string movie)
    {
        return Path.GetExtension(movie);
    }

    public string LoadBytes(string filePath)
    {
        try
        {
            var result =  File.ReadAllBytes(filePath);
            return Convert.ToBase64String(result);
        }
        catch (IOException ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return string.Empty;
        }
    }
}
