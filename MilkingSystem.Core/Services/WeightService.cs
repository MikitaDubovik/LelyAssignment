using MilkingSystem.Core.Models;
using MilkingSystem.Core.Repositories;
using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public class WeightService(
    IAnimalRepository animalRepository,
    IRobotRepository robotRepository,
    IWeightMeasurementRepository weightMeasurementRepository) : IWeightService
{
    private readonly IAnimalRepository _animalRepository = animalRepository;
    private readonly IRobotRepository _robotRepository = robotRepository;
    private readonly IWeightMeasurementRepository _weightMeasurementRepository = weightMeasurementRepository;

    public List<WeightMeasurement> GetWeightMeasurementsForAnimal(int animalId)
        => _weightMeasurementRepository.GetWeightMeasurementsForAnimal(animalId);

    public WeightMeasurement? GetLastWeightForAnimal(int animalId)
        => _weightMeasurementRepository.GetLastWeightForAnimal(animalId);

    public WeightServiceResult RecordWeight(int animalId, int robotId, decimal weightKg, DateTime? timestamp)
    {
        if (weightKg <= 0)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.WeightIsIncorrect };
        }

        var animal = _animalRepository.GetAnimalById(animalId);
        if (animal is null)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.AnimalNotFound };
        }

        var robot = _robotRepository.GetRobotById(robotId);
        if (robot is null)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.RobotNotFound };
        }
        if (!robot.IsActive)
        {
            return new WeightServiceResult { Status = WeightServiceStatus.RobotNotActive };
        }

        var effectiveTimestamp = timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
        var id = _weightMeasurementRepository.SaveWeightMeasurement(animalId, robotId, effectiveTimestamp, weightKg);

        return new WeightServiceResult { Status = WeightServiceStatus.Success, MeasurementId = id };
    }
}
