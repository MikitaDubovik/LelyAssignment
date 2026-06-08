namespace MilkingSystem.Core.Models;

public class WeightMeasurement
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public int RobotId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal WeightKg { get; set; }
    public DateTime CreatedAt { get; set; }
}
