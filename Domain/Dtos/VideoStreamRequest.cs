namespace Domain.Dtos;

public class VideoStreamRequest
{
    public int SegmentFrom { get; set; }
    public int SegmentTo { get; set; }
    public required int MovieId { get; set; }
    public int LastSegment { get; set; } // Add this property
    
}
