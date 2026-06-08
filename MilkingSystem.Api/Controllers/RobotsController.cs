using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController : ControllerBase
{
    private readonly DataService _dataService;

    public RobotsController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_dataService.GetAllRobots());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var robot = _dataService.GetRobotById(id);
        if (robot == null)
            return NotFound();
        return Ok(robot);
    }

    [HttpGet("active")]
    public IActionResult GetActive()
    {
        return Ok(_dataService.GetActiveRobots());
    }
}
