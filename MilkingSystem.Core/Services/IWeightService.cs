using MilkingSystem.Core.Models;
using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public interface IWeightService
{
    /// <summary>Returns all weight measurements for the given animal.</summary>
    List<WeightMeasurement> GetWeightMeasurementsForAnimal(int animalId);

    /// <summary>Returns the most recent weight measurement for the given animal, or null if none exists.</summary>
    WeightMeasurement? GetLastWeightForAnimal(int animalId);

    /// <summary>
    /// Records a weight measurement for an animal.
    /// Validates the animal and robot, then persists the measurement.
    /// </summary>
    WeightServiceResult RecordWeight(int animalId, int robotId, decimal weightKg, DateTime? timestamp);
}
