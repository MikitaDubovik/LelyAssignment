namespace MilkingSystem.Core.Models;

public class Animal
{
    public int Id { get; set; }
    public string IdentificationNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
