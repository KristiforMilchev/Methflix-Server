using System;

namespace Domain.Models;

public partial class SiteCategoryRoute
{
    public int Id { get; set; }

    public int SiteConnectorId { get; set; }

    public int CategoryId { get; set; }

    public string Label { get; set; } = null!;

    public string PathTemplate { get; set; } = null!;

    public string? CreatedAt { get; set; }

    public virtual SiteConnector SiteConnector { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
