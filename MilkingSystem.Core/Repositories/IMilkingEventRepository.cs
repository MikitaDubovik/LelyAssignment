using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public interface IMilkingEventRepository
{
    /// <summary>Returns all milking events for the given animal, ordered by timestamp descending.</summary>
    List<MilkingEvent> GetMilkingEventsForAnimal(int animalId);

    /// <summary>Returns the most recent milking event for the given animal, or null if none exists.</summary>
    MilkingEvent? GetLastMilkingForAnimal(int animalId);

    /// <summary>Saves a new milking event and returns its generated ID.</summary>
    int SaveMilkingEvent(int animalId, int robotId, DateTime timestamp, decimal milkYieldLiters, int? duration);

    /// <summary>Returns all milking events that occurred within the last <paramref name="hours"/> hours.</summary>
    List<MilkingEvent> GetRecentMilkingEvents(int hours = 24);

    /// <summary>Returns the total milk yield per animal within the given time range.</summary>
    Dictionary<int, decimal> GetTotalMilkYieldByAnimal(DateTime from, DateTime to);

    /// <summary>Returns the average milk yield in litres for the given animal across all recorded events.</summary>
    double GetAverageMilkYield(int animalId);

    /// <summary>
    /// Returns a per-animal semaphore used to serialise the check-then-save operation
    /// for a single animal while allowing concurrent operations on different animals.
    /// </summary>
    SemaphoreSlim GetAnimalMilkingLock(int animalId);
}
