using MonoTorrent.Client;

namespace Domain.Dtos;

public class ActiveTorrent
{
    public string Name { get; set; }
    public TorrentState State { get; set; }
    public double Percentage { get; set; }
    public bool IsInitialSeeding { get; set; }
    public int Seeds { get; set; }
    public int Peers { get; set; }
    public double CurrentDownloadSpeed { get; set; }
    public double UploadSpeed { get; set; }
}
