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

    // TODO: Candidate should implement this endpoint
    // [HttpPost]
    // public IActionResult RecordWeight([FromBody] RecordWeightRequest request)
    // {
    //     // Implementation needed:
    //     // 1. Validate the request
    //     // 2. Check if animal exists
    //     // 3. Check if robot exists and is active
    //     // 4. Save the weight measurement
    // }
}
