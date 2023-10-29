using Domain.Models;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

namespace Application.Services;

public class CdnService : ICdnService
{
    private readonly IMovieRepository _movieRepository;

    public CdnService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<string> Add(int id)
    {
        var movie = await _movieRepository.GetMovieById(id);
        var exists = movie != null;
        if (!exists) return string.Empty;

        General.Movies.Add(movie!);
        return movie!.Path;
    }

    public async Task<Movie> PathExists(int id)
    {
        var result = General.Movies.FirstOrDefault(x => x.Id == id);
        if (result != null) return result;
        
        var res = await Add(id);
        return General.Movies.FirstOrDefault(x => x.Id == id)!;
    }

    public List<Movie> GetAllActive()
    {
        return General.Movies;
    }

    public bool Dispose()
    {
        General.Movies = new List<Movie>();
        return true;
    }
}
