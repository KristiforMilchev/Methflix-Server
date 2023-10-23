namespace Domain.Models;

public class AccessKey : BaseEntity
{
    public string Key { get; set; }
    public int AccountId { get; set; }
    public Account Account { get; set; }
}
