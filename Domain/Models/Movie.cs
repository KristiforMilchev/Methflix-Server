namespace Domain.Models;

public class Movie : BaseEntity
{
    public string Name { get; set; }
    public string Location { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
