namespace Domain.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public Account CreatedByUser { get; set; }
}
