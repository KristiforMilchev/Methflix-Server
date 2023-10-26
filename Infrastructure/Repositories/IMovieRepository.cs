using Domain.Models;

namespace Infrastructure.Repositories;

public interface IMovieRepository
{
    public Task<List<Movie>> GetAllMovies();
    public Task<Category?> GetCategoryMovies(int categoryId);
    public Task<Movie?> GetMovieById(int id);
}
