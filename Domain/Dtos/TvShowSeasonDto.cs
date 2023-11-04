namespace Domain.Dtos;

public class TvShowSeasonDto
{
    public TvShowSeasonDto()
    {
        Seasons = new List<SeasonData>();
    }
    public string Name { get; set; }
    public List<SeasonData> Seasons { get; set; }
}
