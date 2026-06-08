using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Services;

public interface IAnimalService
{
    /// <summary>Returns all animals in the system.</summary>
    Task<List<Animal>> GetAllAnimals();

    /// <summary>Returns the animal with the given ID, or null if not found.</summary>
    Task<Animal?> GetAnimalById(int id);

    /// <summary>Returns the animal with the given identification number, or null if not found.</summary>
    Task<Animal?> GetAnimalByIdentificationNumber(string identificationNumber);

    /// <summary>Creates a new animal and returns its generated ID.</summary>
    Task<int> CreateAnimal(string identificationNumber, string? name, DateTime? birthDate);
}
