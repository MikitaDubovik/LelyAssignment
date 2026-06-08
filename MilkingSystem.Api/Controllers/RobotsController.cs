using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController(DataService dataService) : ControllerBase
{
    private readonly DataService _dataService = dataService;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_dataService.GetAllRobots());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var robot = _dataService.GetRobotById(id);
        if (robot is null)
        {
            return NotFound();
        }
        return Ok(robot);
    }

    [HttpGet("active")]
    public IActionResult GetActive()
    {
        return Ok(_dataService.GetActiveRobots());
    }
}
