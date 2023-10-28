using Domain.Models;

namespace Infrastructure.Interfaces;

public interface ICdnService
{
    public Task<string> Add(int id);
    public Movie? PathExists(int id);
    public List<Movie> GetAllActive();
    public bool Dispose();
}
