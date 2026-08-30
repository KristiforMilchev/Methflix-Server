using System;
using System.Collections.Generic;

namespace Domain.Models;

public partial class SiteConnector
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string BaseUrl { get; set; } = null!;

    public string TopListingPath { get; set; } = null!;

    public PageFetchStrategy FetchStrategy { get; set; }

    public string RowSelector { get; set; } = null!;

    public string TitleSelector { get; set; } = null!;

    public string DetailLinkSelector { get; set; } = null!;

    public string MagnetSelector { get; set; } = null!;

    public int CreatedBy { get; set; }

    public string? CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<SiteCategoryRoute> CategoryRoutes { get; set; } = new List<SiteCategoryRoute>();
}
