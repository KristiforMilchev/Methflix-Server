using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class TvShow
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Season { get; set; }

    public int? CreatedBy { get; set; }

    public string? CreatedAt { get; set; }

    public virtual ICollection<AssociatedSeasonEpisode> AssociatedSeasonEpisodes { get; set; } = new List<AssociatedSeasonEpisode>();

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
