using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Movie
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public TimeSpan TimeData { get; set; }

    public string Path { get; set; } = null!;

    public int CategoryId { get; set; }

    public int? TorrentId { get; set; }

    public int? DownloadId { get; set; }

    public int? Extension { get; set; }

    public string? Thumbnail { get; set; }

    public int? TvShowId { get; set; }

    public virtual ICollection<AssociatedSeasonEpisode> AssociatedSeasonEpisodes { get; set; } = new List<AssociatedSeasonEpisode>();

    public virtual Category Category { get; set; } = null!;

    public virtual Upload? Download { get; set; }

    public virtual FileExtension? ExtensionNavigation { get; set; }

    public virtual Dtorrent? Torrent { get; set; }

    public virtual TvShow? TvShow { get; set; }
}
