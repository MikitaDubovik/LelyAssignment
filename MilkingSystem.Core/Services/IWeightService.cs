using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public interface IWeightService
{
    /// <summary>
    /// Records a weight measurement for an animal.
    /// </summary>
    WeightServiceResult RecordWeight(int animalId, int robotId, decimal weightKg, DateTime? timestamp);
}
