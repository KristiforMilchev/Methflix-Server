using Microsoft.AspNetCore.Http;

namespace Domain.Dtos;

public class MovieResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Thumbnail { get; set; }
    public TimeSpan Lenght { get; set; }
    public IFormFile File { get; set; }
}
