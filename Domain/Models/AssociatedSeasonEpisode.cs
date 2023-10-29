using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class AssociatedSeasonEpisode
{
    public int Id { get; set; }

    public int? TvShowId { get; set; }

    public int? MovieId { get; set; }

    public int? Season { get; set; }

    public string? CreatedAt { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual TvShow? TvShow { get; set; }
}
