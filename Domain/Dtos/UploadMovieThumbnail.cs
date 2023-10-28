using Microsoft.AspNetCore.Http;

namespace Domain.Dtos;

public class UploadMovieThumbnail
{
    public int Id { get; set; }
    public IFormFile File { get; set; }
}
