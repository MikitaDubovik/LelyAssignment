namespace MilkingSystem.Api.Models;

/// <summary>
/// Request model for recording a weight measurement.
/// </summary>
public class RecordWeightRequest
{
    /// <summary>
    /// The ID of the animal being weighed.
    /// </summary>
    public int AnimalId { get; set; }

    /// <summary>
    /// The ID of the robot performing the weighing.
    /// </summary>
    public int RobotId { get; set; }

    /// <summary>
    /// The weight of the animal in kilograms.
    /// </summary>
    public decimal WeightKg { get; set; }

    /// <summary>
    /// The timestamp of the measurement. If not provided, current UTC time will be used.
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
