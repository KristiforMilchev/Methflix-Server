using Domain.Context;
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
        return await _context.Categories.Include(x => x.Movies).ToListAsync();
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

    public async Task<Movie?> GetMovieById(int id)
    {
        return await _context.Movies.FirstOrDefaultAsync(x => x.Id == id);
    }
}
