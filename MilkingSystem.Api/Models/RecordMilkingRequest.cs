namespace MilkingSystem.Api.Models;

/// <summary>
/// Request model for recording a milking event.
/// </summary>
public class RecordMilkingRequest
{
    /// <summary>
    /// The ID of the animal being milked.
    /// </summary>
    public int AnimalId { get; set; }

    /// <summary>
    /// The ID of the robot performing the milking.
    /// </summary>
    public int RobotId { get; set; }

    /// <summary>
    /// The amount of milk collected in liters.
    /// </summary>
    public decimal MilkYieldLiters { get; set; }

    /// <summary>
    /// The duration of the milking in seconds (optional).
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// The timestamp of the milking event. If not provided, current UTC time will be used.
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
