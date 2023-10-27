namespace Domain.Dtos;

public class VideoStreamResponse
{
    public string ChunkData { get; set; }
    public int NextChunk { get; set; }
    public string Extension { get; set; }
}
