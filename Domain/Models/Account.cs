using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class Account
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int RoleId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<AccessKey> AccessKeys { get; set; } = new List<AccessKey>();

    public virtual ICollection<Dtorrent> Dtorrents { get; set; } = new List<Dtorrent>();

    public virtual ICollection<FileExtension> FileExtensions { get; set; } = new List<FileExtension>();

    public virtual AccessRole Role { get; set; } = null!;

    public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();
}
