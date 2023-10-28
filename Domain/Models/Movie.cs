using System;
using System.Collections.Generic;
using System.Reflection;

namespace Domain.Models;

public partial class Movie
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public TimeSpan TimeData { get; set; }

    public string Path { get; set; } = null!;
    
    public string? Thumbnail { get; set; }
    public int CategoryId { get; set; }

    public int? TorrentId { get; set; }

    public int? DownloadId { get; set; }

    public int? Extension { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Upload? Download { get; set; }

    public virtual FileExtension? ExtensionNavigation { get; set; }

    public virtual Dtorrent? Torrent { get; set; }
}
