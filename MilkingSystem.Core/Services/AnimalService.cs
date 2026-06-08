using MilkingSystem.Core.Models;
using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Core.Services;

public class AnimalService(IAnimalRepository animalRepository) : IAnimalService
{
    private readonly IAnimalRepository _animalRepository = animalRepository;

    public Task<List<Animal>> GetAllAnimals()
        => _animalRepository.GetAllAnimals();

    public Task<Animal?> GetAnimalById(int id)
        => _animalRepository.GetAnimalById(id);

    public Task<Animal?> GetAnimalByIdentificationNumber(string identificationNumber)
        => _animalRepository.GetAnimalByIdentificationNumber(identificationNumber);

    public Task<int> CreateAnimal(string identificationNumber, string? name, DateTime? birthDate)
        => _animalRepository.CreateAnimal(identificationNumber, name, birthDate);
}
