namespace Domain.Dtos;

public class VideoStreamRequest
{
    public int SegmentFrom { get; set; }
    public int SegmentTo { get; set; }
    public required string Movie { get; set; }
    
}
