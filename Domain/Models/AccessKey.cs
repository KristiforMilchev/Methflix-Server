using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class AccessKey
{
    public int Id { get; set; }

    public string? Key { get; set; }

    public int? AccountId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? RemovedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Account? Account { get; set; }
}
