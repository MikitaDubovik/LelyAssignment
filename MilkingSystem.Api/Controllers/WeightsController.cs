using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Repositories;
using MilkingSystem.Core.Results;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

/// <summary>
/// Controller for weight measurements.
///
/// TODO: Candidates should implement POST endpoint for recording weight measurements.
///
/// Requirements:
/// - Accept weight data from robots (animalId, robotId, weightKg)
/// - Validate that animal and robot exist
/// - There is no "double-weighing" protection needed (unlike milking)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeightsController(IWeightMeasurementRepository weightMeasurementRepository, IWeightService weightService) : ControllerBase
{
    private readonly IWeightMeasurementRepository _weightMeasurementRepository = weightMeasurementRepository;
    private readonly IWeightService _weightService = weightService;

    [HttpGet("animal/{animalId}")]
    public IActionResult GetForAnimal(int animalId)
    {
        return Ok(_weightMeasurementRepository.GetWeightMeasurementsForAnimal(animalId));
    }

    [HttpGet("animal/{animalId}/last")]
    public IActionResult GetLastForAnimal(int animalId)
    {
        var lastMeasurement = _weightMeasurementRepository.GetLastWeightForAnimal(animalId);
        if (lastMeasurement is null)
        {
            return NotFound();
        }
        return Ok(lastMeasurement);
    }

    [HttpPost]
    public IActionResult RecordWeight([FromBody] RecordWeightRequest request)
    {
        var result = _weightService.RecordWeight(
            request.AnimalId, request.RobotId,
            request.WeightKg, request.Timestamp);

        return result.Status switch
        {
            WeightServiceStatus.WeightIsIncorrect => BadRequest(new { error = "WeightKg must be greater than zero" }),
            WeightServiceStatus.Success => Ok(new { id = result.MeasurementId }),
            WeightServiceStatus.AnimalNotFound => NotFound(new { error = "Animal not found" }),
            WeightServiceStatus.RobotNotFound => NotFound(new { error = "Robot not found" }),
            WeightServiceStatus.RobotNotActive => UnprocessableEntity(new { error = "Robot is not active" }),
            _ => StatusCode(500, new { error = "Unexpected error" })
        };
    }
}
