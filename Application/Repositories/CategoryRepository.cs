using Domain.Context;
using Domain.Models;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly MethflixContext _context;
    private readonly IMovieRepository _movieRepository;

    public CategoryRepository(MethflixContext context ,IMovieRepository movieRepository)
    {
        _context = context;
        _movieRepository = movieRepository;
    }
    
    public async Task<Category?> GetCategory(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
    }
    
    
    public async Task<List<Category>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }
    
    public async Task<bool> UpdateCategory(Category category)
    {
        try
        {
            _context.Attach(category);
            _context.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> AddCategory(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            return true;
        }
    }

    public async Task<bool> RemoveCategory(int id)
    {
        var categoryMovies = await _movieRepository.GetCategoryMovies(id);
        if (categoryMovies == null) return false;
        
        foreach (var movie in categoryMovies.Movies)
        {
            movie.CategoryId = 1;
            await _movieRepository.UpdateMovie(movie);
        }

        var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
        _context.Categories.Remove(category!);
        return true;
    }
}
