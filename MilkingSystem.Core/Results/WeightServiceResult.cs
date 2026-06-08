namespace MilkingSystem.Core.Results;

public record WeightServiceResult
{
    public WeightServiceStatus Status { get; init; }
    public int? MeasurementId { get; init; }
}
