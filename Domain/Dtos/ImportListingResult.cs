namespace Domain.Dtos;

public class ImportListingResult
{
    public bool Started { get; set; }
    public string? Magnet { get; set; }
    public string DetailUrl { get; set; } = null!;
    public string Message { get; set; } = null!;
}
