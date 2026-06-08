using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Notifications;
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
public class MilkingsController(DataService dataService, IRobotNotifier notifier) : ControllerBase
{
    private readonly DataService _dataService = dataService;
    private readonly IRobotNotifier _notifier = notifier;

    [HttpGet("animal/{animalId}")]
    public IActionResult GetForAnimal(int animalId)
    {
        return Ok(_dataService.GetMilkingEventsForAnimal(animalId));
    }

    [HttpGet("animal/{animalId}/last")]
    public IActionResult GetLastForAnimal(int animalId)
    {
        var lastEvent = _dataService.GetLastMilkingForAnimal(animalId);
        if (lastEvent is null)
        {
            return NotFound();
        }
        return Ok(lastEvent);
    }

    [HttpGet("recent")]
    public IActionResult GetRecent([FromQuery] int hours = 24)
    {
        return Ok(_dataService.GetRecentMilkingEvents(hours));
    }

    [HttpPost]
    public async Task<IActionResult> RecordMilking([FromBody] RecordMilkingRequest request)
    {
        if (request.MilkYieldLiters <= 0)
        {
            return BadRequest(new { error = "MilkYieldLiters must be greater than zero" });
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

        // Per-animal lock: different animals can be processed in parallel;
        // the same animal is serialised so the check-then-save is atomic.
        var animalLock = _dataService.GetAnimalMilkingLock(request.AnimalId);
        await animalLock.WaitAsync();
        try
        {
            if (_notifier.WasRecentlyMilked(request.AnimalId))
            {
                var lastMilking = _dataService.GetLastMilkingForAnimal(request.AnimalId);
                return Conflict(new
                {
                    error = "Animal was milked too recently",
                    lastMilkedAt = lastMilking?.Timestamp,
                    nextAllowedAt = lastMilking?.Timestamp.AddHours(6)
                });
            }

            var id = _dataService.SaveMilkingEvent(
                request.AnimalId, request.RobotId, timestamp,
                request.MilkYieldLiters, request.Duration);

            _notifier.NotifyMilkingCompleted(new MilkingNotification
            {
                AnimalId = request.AnimalId,
                RobotId = request.RobotId,
                Timestamp = timestamp,
                AnimalIdentificationNumber = animal.IdentificationNumber
            });

            return Ok(new { id });
        }
        finally
        {
            animalLock.Release();
        }
    }
}
