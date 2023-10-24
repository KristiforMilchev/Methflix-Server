using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class FileExtension
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int AddedBy { get; set; }

    public virtual Account AddedByNavigation { get; set; } = null!;

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
