namespace MilkingSystem.Core.Notifications;

/// <summary>
/// Notification sent to robots when a milking event is completed.
/// This is used to prevent double-milking of the same animal.
/// </summary>
public class MilkingNotification
{
    public int AnimalId { get; set; }
    public int RobotId { get; set; }
    public DateTime Timestamp { get; set; }
    public string AnimalIdentificationNumber { get; set; } = string.Empty;
}
