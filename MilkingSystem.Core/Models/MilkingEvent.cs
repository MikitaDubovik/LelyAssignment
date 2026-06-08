namespace MilkingSystem.Core.Models;

public class MilkingEvent
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public int RobotId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal MilkYieldLiters { get; set; }
    public int? Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}
