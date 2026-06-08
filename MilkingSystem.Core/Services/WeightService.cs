using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public class WeightService(DataService dataService) : IWeightService
{
    private readonly DataService _dataService = dataService;

    public WeightServiceResult RecordWeight(int animalId, int robotId, decimal weightKg, DateTime? timestamp)
    {
        if (weightKg <= 0)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.WeightIsIncorrect };
        }

        var animal = _dataService.GetAnimalById(animalId);
        if (animal is null)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.AnimalNotFound };
        }

        var robot = _dataService.GetRobotById(robotId);
        if (robot is null)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.RobotNotFound };
        }
        if (!robot.IsActive)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.RobotNotActive };
        }

        var effectiveTimestamp = timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
        var id = _dataService.SaveWeightMeasurement(animalId, robotId, effectiveTimestamp, weightKg);

        return new WeightServiceResult { Status = WeightServiceStatus.Success, MeasurementId = id };
    }
}
