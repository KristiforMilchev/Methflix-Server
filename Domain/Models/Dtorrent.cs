using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Dtorrent
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int RequestedBy { get; set; }

    public string? Location { get; set; }

    public bool IsSeeding { get; set; }

    public bool IsDownloaded { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();

    public virtual Account RequestedByNavigation { get; set; } = null!;
}
