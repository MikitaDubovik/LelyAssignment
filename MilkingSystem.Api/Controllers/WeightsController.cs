using Autofac;
using Microsoft.AspNetCore.Mvc;
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
public class WeightsController : ControllerBase
{
    private readonly ILifetimeScope _scope;

    public WeightsController(ILifetimeScope scope)
    {
        _scope = scope;
    }

    [HttpGet("animal/{animalId}")]
    public IActionResult GetForAnimal(int animalId)
    {
        var dataService = _scope.Resolve<DataService>();
        var measurements = dataService.GetWeightMeasurementsForAnimal(animalId);
        return Ok(measurements);
    }

    [HttpGet("animal/{animalId}/last")]
    public IActionResult GetLastForAnimal(int animalId)
    {
        var dataService = _scope.Resolve<DataService>();
        var lastMeasurement = dataService.GetLastWeightForAnimal(animalId);
        
        if (lastMeasurement == null)
            return NotFound();
        
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

/// <summary>
/// Request model for recording a weight measurement.
/// </summary>
public class RecordWeightRequest
{
    /// <summary>
    /// The ID of the animal being weighed.
    /// </summary>
    public int AnimalId { get; set; }
    
    /// <summary>
    /// The ID of the robot performing the weighing.
    /// </summary>
    public int RobotId { get; set; }
    
    /// <summary>
    /// The weight of the animal in kilograms.
    /// </summary>
    public decimal WeightKg { get; set; }
    
    /// <summary>
    /// The timestamp of the measurement. If not provided, current UTC time will be used.
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
