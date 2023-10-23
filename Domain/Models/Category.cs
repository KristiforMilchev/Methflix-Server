namespace Domain.Models;

public class Category : BaseEntity
{
    public Category()
    {
        Movies = new List<Movie>();
    }
    
    public string Name { get; set; } 
    public ICollection<Movie> Movies { get; set; }
}
