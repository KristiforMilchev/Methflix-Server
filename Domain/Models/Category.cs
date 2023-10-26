using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CreatedBy { get; set; }

    public string CreatedAt { get; set; } = null!;

    public int? UpdatedBy { get; set; }

    public string? UpdatedAt { get; set; }

    public int? IsDeleted { get; set; }

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();

    public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
}
