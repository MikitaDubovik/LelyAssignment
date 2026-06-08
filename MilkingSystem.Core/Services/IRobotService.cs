using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Services;

public interface IRobotService
{
    /// <summary>Returns all robots in the system.</summary>
    Task<List<Robot>> GetAllRobots();

    /// <summary>Returns the robot with the given ID, or null if not found.</summary>
    Task<Robot?> GetRobotById(int id);

    /// <summary>Returns all currently active robots.</summary>
    Task<List<Robot>> GetActiveRobots();
}
