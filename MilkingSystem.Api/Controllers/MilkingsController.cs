using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

/// <summary>
/// Controller for milking events.
/// 
/// TODO: Candidates should implement POST endpoint for recording new milking events.
/// 
/// Background:
/// - Robots are large stationary machines in the barn
/// - A cow walks into a robot and gets milked autonomously
/// - After milking, a cow might walk to another robot hoping for more food
/// - Other robots must know NOT to milk this cow (she was recently milked)
/// 
/// Requirements:
/// - Accept milking data from robots (animalId, robotId, milkYieldLiters, duration)
/// - Prevent double-milking: if an animal was milked within the last 6 hours, reject the request
/// - Notify other robots about the milking event using IRobotNotifier
/// - Handle concurrent messages from multiple robots about different cows
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MilkingsController : ControllerBase
{
    private readonly DataService _dataService;

    public MilkingsController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet("animal/{animalId}")]
    public IActionResult GetForAnimal(int animalId)
    {
        return Ok(_dataService.GetMilkingEventsForAnimal(animalId));
    }

    [HttpGet("animal/{animalId}/last")]
    public IActionResult GetLastForAnimal(int animalId)
    {
        var lastEvent = _dataService.GetLastMilkingForAnimal(animalId);
        if (lastEvent == null)
            return NotFound();
        return Ok(lastEvent);
    }

    [HttpGet("recent")]
    public IActionResult GetRecent([FromQuery] int hours = 24)
    {
        return Ok(_dataService.GetRecentMilkingEvents(hours));
    }

    // TODO: Candidate should implement this endpoint
    // [HttpPost]
    // public IActionResult RecordMilking([FromBody] RecordMilkingRequest request)
    // {
    //     // Implementation needed:
    //     // 1. Validate the request
    //     // 2. Check if animal exists
    //     // 3. Check if robot exists and is active
    //     // 4. Check if animal was recently milked (within 6 hours) - use IRobotNotifier.WasRecentlyMilked
    //     // 5. Save the milking event
    //     // 6. Notify other robots using IRobotNotifier.NotifyMilkingCompleted
    //     // 7. Handle concurrency (what if two robots try to milk same animal at same time?)
    // }
}

/// <summary>
/// Request model for recording a milking event.
/// </summary>
public class RecordMilkingRequest
{
    /// <summary>
    /// The ID of the animal being milked.
    /// </summary>
    public int AnimalId { get; set; }
    
    /// <summary>
    /// The ID of the robot performing the milking.
    /// </summary>
    public int RobotId { get; set; }
    
    /// <summary>
    /// The amount of milk collected in liters.
    /// </summary>
    public decimal MilkYieldLiters { get; set; }
    
    /// <summary>
    /// The duration of the milking in seconds (optional).
    /// </summary>
    public int? Duration { get; set; }
    
    /// <summary>
    /// The timestamp of the milking event. If not provided, current UTC time will be used.
    /// </summary>
    public DateTime? Timestamp { get; set; }
}
