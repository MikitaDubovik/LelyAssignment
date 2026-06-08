using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
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
public class WeightsController(DataService dataService) : ControllerBase
{
    private readonly DataService _dataService = dataService;

    [HttpGet("animal/{animalId}")]
    public IActionResult GetForAnimal(int animalId)
    {
        return Ok(_dataService.GetWeightMeasurementsForAnimal(animalId));
    }

    [HttpGet("animal/{animalId}/last")]
    public IActionResult GetLastForAnimal(int animalId)
    {
        var lastMeasurement = _dataService.GetLastWeightForAnimal(animalId);
        if (lastMeasurement is null)
        {
            return NotFound();
        }
        return Ok(lastMeasurement);
    }

    [HttpPost]
    public IActionResult RecordWeight([FromBody] RecordWeightRequest request)
    {
        if (request.WeightKg <= 0)
        {
            return BadRequest(new { error = "WeightKg must be greater than zero" });
        }

        var animal = _dataService.GetAnimalById(request.AnimalId);
        if (animal is null)
        {
            return NotFound(new { error = "Animal not found" });
        }

        var robot = _dataService.GetRobotById(request.RobotId);
        if (robot is null)
        {
            return NotFound(new { error = "Robot not found" });
        }
        if (!robot.IsActive)
        {
            return UnprocessableEntity(new { error = "Robot is not active" });
        }

        var timestamp = request.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
        var id = _dataService.SaveWeightMeasurement(request.AnimalId, request.RobotId, timestamp, request.WeightKg);
        return Ok(new { id });
    }
}
