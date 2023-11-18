namespace Infrastructure.Interfaces;

public interface ITvShowsRepository
{
    public Task<string> TvShowName(int id);
}
