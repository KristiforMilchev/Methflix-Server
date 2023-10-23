using System.Reflection;

namespace Domain.Models;

public class DTorrent : BaseEntity
{
    public string Name { get; set; }
    public Account RequestedBy { get; set; }
    public int RequestedById { get; set; }
    public string Location { get; set; }
    public bool IsSeeding { get; set; }
    public bool IsDownloading { get; set; }
}
