using Domain.Models;

namespace Domain.Dtos;

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<MovieResponseDto> Movies { get; set; }
    public List<TvShowDto> TvShows { get; set; }
}
