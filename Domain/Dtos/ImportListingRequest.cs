namespace Domain.Dtos;

public class ImportListingRequest
{
    public int SiteConnectorId { get; set; }
    public string DetailUrl { get; set; } = null!;
    public int? CategoryId { get; set; }
}
