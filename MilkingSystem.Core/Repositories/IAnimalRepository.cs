using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public interface IAnimalRepository
{
    /// <summary>Returns all animals in the system.</summary>
    List<Animal> GetAllAnimals();

    /// <summary>Returns the animal with the given ID, or null if not found.</summary>
    Animal? GetAnimalById(int id);

    /// <summary>Returns the animal with the given identification number, or null if not found.</summary>
    Animal? GetAnimalByIdentificationNumber(string identificationNumber);

    /// <summary>Creates a new animal and returns its generated ID.</summary>
    int CreateAnimal(string identificationNumber, string? name, DateTime? birthDate);
}
