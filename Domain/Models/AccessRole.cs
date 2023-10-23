namespace Domain.Models;

public class AccessRole : BaseEntity
{
    public string Name { get; set; }
    public int Priority { get; set; }
    public IEnumerable<Account> Accounts { get; }
}
