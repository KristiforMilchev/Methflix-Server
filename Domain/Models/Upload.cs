using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Upload
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int CategoryId { get; set; }

    public string Path { get; set; } = null!;

    public int RequestedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();

    public virtual Account RequestedByNavigation { get; set; } = null!;
}
