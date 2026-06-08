using System.ComponentModel.DataAnnotations;

namespace MilkingSystem.Api.Models;

/// <summary>
/// Request model for recording a weight measurement.
/// </summary>
public class RecordWeightRequest
{
    /// <summary>
    /// The ID of the animal being weighed.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int AnimalId { get; set; }

    /// <summary>
    /// The ID of the robot performing the weighing.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int RobotId { get; set; }

    /// <summary>
    /// The weight of the animal in kilograms.
    /// </summary>
    [Range(typeof(decimal), "0.001", "1000000")]
    public decimal WeightKg { get; set; }

    /// <summary>
    /// The timestamp of the measurement. If not provided, current UTC time will be used.
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
