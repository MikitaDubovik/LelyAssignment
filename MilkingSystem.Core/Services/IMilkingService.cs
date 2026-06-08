using MilkingSystem.Core.Models;
using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public interface IMilkingService
{
    /// <summary>Returns all milking events for the given animal, ordered by timestamp descending.</summary>
    List<MilkingEvent> GetMilkingEventsForAnimal(int animalId);

    /// <summary>Returns the most recent milking event for the given animal, or null if none exists.</summary>
    MilkingEvent? GetLastMilkingForAnimal(int animalId);

    /// <summary>Returns all milking events that occurred within the last <paramref name="hours"/> hours.</summary>
    List<MilkingEvent> GetRecentMilkingEvents(int hours = 24);

    /// <summary>
    /// Records a milking event for an animal.
    /// Validates the animal and robot, enforces the double-milking protection window,
    /// persists the event, and notifies other robots via IRobotNotifier.
    /// </summary>
    Task<MilkingServiceResult> RecordMilking(
        int animalId, int robotId, decimal milkYieldLiters, int? duration, DateTime? timestamp);
}
