using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Results;
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
public class MilkingsController(IMilkingService milkingService) : ControllerBase
{
    private readonly IMilkingService _milkingService = milkingService;

    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> GetForAnimal(int animalId)
    {
        return Ok(await _milkingService.GetMilkingEventsForAnimal(animalId));
    }

    [HttpGet("animal/{animalId}/last")]
    public async Task<IActionResult> GetLastForAnimal(int animalId)
    {
        var lastEvent = await _milkingService.GetLastMilkingForAnimal(animalId);
        if (lastEvent is null)
        {
            return NotFound();
        }
        return Ok(lastEvent);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int hours = 24)
    {
        return Ok(await _milkingService.GetRecentMilkingEvents(hours));
    }

    [HttpPost]
    public async Task<IActionResult> RecordMilking([FromBody] RecordMilkingRequest request)
    {
        var result = await _milkingService.RecordMilking(
            request.AnimalId, request.RobotId,
            request.MilkYieldLiters, request.Duration,
            request.Timestamp);

        return result.Status switch
        {
            MilkingServiceStatus.MilkAmountIsIncorrect => BadRequest(new { error = "MilkYieldLiters must be greater than zero" }),
            MilkingServiceStatus.Success => CreatedAtAction(nameof(GetForAnimal), new { animalId = request.AnimalId }, new { id = result.EventId }),
            MilkingServiceStatus.AnimalNotFound => NotFound(new { error = "Animal not found" }),
            MilkingServiceStatus.RobotNotFound => NotFound(new { error = "Robot not found" }),
            MilkingServiceStatus.RobotNotActive => UnprocessableEntity(new { error = "Robot is not active" }),
            MilkingServiceStatus.RecentlyMilked => Conflict(new
            {
                error = "Animal was milked too recently",
                lastMilkedAt = result.LastMilkedAt,
                nextAllowedAt = result.NextAllowedAt
            }),
            _ => StatusCode(500, new { error = "Unexpected error" })
        };
    }
}
