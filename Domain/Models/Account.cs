namespace Domain.Models;

public class Account : BaseEntity
{
    public Account()
    {
        Keys = new List<AccessKey>();
    }
    
    public string Name { get; set; }
    public int RoleId { get; set; } 
    public ICollection<AccessKey> Keys { get; set; }
    public int AccessRoleId { get; set; }
    public AccessRole AccessRole { get; set; }
    public IEnumerable<DTorrent> DTorrents { get; set; }
}
