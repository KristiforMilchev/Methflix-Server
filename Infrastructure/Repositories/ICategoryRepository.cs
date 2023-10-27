using Domain.Models;

namespace Infrastructure.Repositories;

public interface ICategoryRepository
{
    public Task<Category?> GetCategory(int id);
    public Task<List<Category>> GetCategories();
    public Task<bool> UpdateCategory(Category category);
    public Task<bool> AddCategory(Category category);
    public Task<bool> RemoveCategory(int id);
}
