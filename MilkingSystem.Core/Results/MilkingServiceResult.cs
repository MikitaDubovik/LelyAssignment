namespace MilkingSystem.Core.Results;

/// <summary>
/// Result returned by <see cref="MilkingSystem.Core.Services.IMilkingService.RecordMilking"/>.
/// On <see cref="MilkingServiceStatus.RecentlyMilked"/>, <see cref="LastMilkedAt"/>
/// and <see cref="NextAllowedAt"/> are populated so the caller can surface them to the client.
/// </summary>
public record MilkingServiceResult
{
    public MilkingServiceStatus Status { get; init; }
    public int? EventId { get; init; }
    public DateTime? LastMilkedAt { get; init; }
    public DateTime? NextAllowedAt { get; init; }
}
