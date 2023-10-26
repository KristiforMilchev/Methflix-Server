using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class AccessRole
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? Priority { get; set; }

    public string? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public int? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
