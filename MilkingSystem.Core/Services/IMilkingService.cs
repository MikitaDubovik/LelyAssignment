using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public interface IMilkingService
{
    /// <summary>
    /// Records a milking event, enforcing the double-milking protection window.
    /// </summary>
    Task<MilkingServiceResult> RecordMilking(
        int animalId, int robotId, decimal milkYieldLiters, int? duration, DateTime? timestamp);
}
