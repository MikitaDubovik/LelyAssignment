namespace MilkingSystem.Core.Configuration;

/// <summary>
/// Feature settings for milking behaviour.
/// </summary>
public class MilkingSettings
{
    /// <summary>
    /// Minimum number of hours that must pass between two milkings of the same animal.
    /// </summary>
    public int ProtectionWindowHours { get; set; } = 6;
}
