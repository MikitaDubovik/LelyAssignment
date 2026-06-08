namespace MilkingSystem.Api.Models;

/// <summary>
/// Request model for creating a new animal.
/// </summary>
public class CreateAnimalRequest
{
    /// <summary>
    /// The unique identification number of the animal (e.g. NL-123456789).
    /// </summary>
    public string IdentificationNumber { get; set; } = string.Empty;

    /// <summary>
    /// The name of the animal (optional).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The date of birth of the animal (optional).
    /// </summary>
    public DateTime? BirthDate { get; set; }
}
