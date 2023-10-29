using Domain.Context;
using Domain.Dtos;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MethflixContext _context;

    public MovieRepository(MethflixContext context)
    {
        _context = context;
    }
    
    public async Task<List<Movie>> GetAllMovies()
    {
       return await _context.Movies.ToListAsync();
    }

    public async Task<Category?> GetCategoryMovies(int categoryId)
    {
        return await _context.Categories.Include(x => x.Movies).FirstOrDefaultAsync(x => x.Id == categoryId);
    }

    public async Task<List<Category>> GetCategoryWithMovies()
    {
        return await _context.Categories.Include(x => x.Movies)
                                        .Where(x=> x.Movies.Any(y=> y.TvShowId == null))
                                        .ToListAsync();
    }

    public async Task<bool> UpdateMovie(Movie movie)
    {
        try
        {
            _context.Attach(movie);
            _context.Update(movie);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<List<TvShow>> GetCategoryTvShows(int id)
    {

         return await _context.TvShows.Include(x => x.Movies)
                                                .Where(x => x.Movies.Any(y => y.CategoryId == id)).
                                                ToListAsync();
    }

    public async Task<TvShowSeasonDto> GetTvShowEpisodesById(int id)
    {
        var tvShow = await _context.AssociatedSeasonEpisodes.Include(x=>x.Movie)
                                                                                    .Where(x=>x.TvShowId == id)
                                                                                    .ToListAsync();
        var tvShowSeasonData = new TvShowSeasonDto();
        var showName = await _context.TvShows.FirstOrDefaultAsync(x => x.Id == id);
        if (showName == null) return new TvShowSeasonDto();
        
        tvShowSeasonData.Name = showName!.Name!;
        foreach (var associatedSeasonEpisodes in tvShow.GroupBy(x => x.Season))
        {
            var season = associatedSeasonEpisodes.Key;
            var movies = new SeasonData
            {
                Season = season ?? 0,
                Movies = associatedSeasonEpisodes.Select(y=> new SeasonMovie
                {
                    Id = y.Movie!.Id,
                    Name = y.Movie.Name
                }).ToList()
            };
            tvShowSeasonData.Seasons.Add(movies);
        }

        return tvShowSeasonData;
    }
    

    public async Task<Movie?> GetMovieById(int id)
    {
        return await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
    }
}
