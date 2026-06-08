using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public interface IMilkingEventRepository
{
    /// <summary>Returns all milking events for the given animal, ordered by timestamp descending.</summary>
    Task<List<MilkingEvent>> GetMilkingEventsForAnimal(int animalId);

    /// <summary>Returns the most recent milking event for the given animal, or null if none exists.</summary>
    Task<MilkingEvent?> GetLastMilkingForAnimal(int animalId);

    /// <summary>Saves a new milking event and returns its generated ID.</summary>
    Task<int> SaveMilkingEvent(int animalId, int robotId, DateTime timestamp, decimal milkYieldLiters, int? duration);

    /// <summary>Returns all milking events that occurred within the last <paramref name="hours"/> hours.</summary>
    Task<List<MilkingEvent>> GetRecentMilkingEvents(int hours = 24);

    /// <summary>Returns the total milk yield per animal within the given time range.</summary>
    Task<Dictionary<int, decimal>> GetTotalMilkYieldByAnimal(DateTime from, DateTime to);

    /// <summary>Returns the average milk yield in litres for the given animal across all recorded events.</summary>
    Task<double> GetAverageMilkYield(int animalId);
}
