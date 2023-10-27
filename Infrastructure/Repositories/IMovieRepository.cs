using Domain.Models;

namespace Infrastructure.Repositories;

public interface IMovieRepository
{
    public Task<List<Movie>> GetAllMovies();
    public Task<Category?> GetCategoryMovies(int categoryId);
    public Task<Movie?> GetMovieById(int id);
    public Task<List<Category>> GetCategoryWithMovies();
    public Task<bool> UpdateMovie(Movie movie);
}
