using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public interface IWeightMeasurementRepository
{
    /// <summary>Returns all weight measurements for the given animal.</summary>
    List<WeightMeasurement> GetWeightMeasurementsForAnimal(int animalId);

    /// <summary>Saves a new weight measurement and returns its generated ID.</summary>
    int SaveWeightMeasurement(int animalId, int robotId, DateTime timestamp, decimal weightKg);

    /// <summary>Returns the most recent weight measurement for the given animal, or null if none exists.</summary>
    WeightMeasurement? GetLastWeightForAnimal(int animalId);
}
