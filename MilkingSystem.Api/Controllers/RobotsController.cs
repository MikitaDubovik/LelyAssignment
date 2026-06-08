using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController(IRobotService robotService) : ControllerBase
{
    private readonly IRobotService _robotService = robotService;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_robotService.GetAllRobots());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var robot = _robotService.GetRobotById(id);
        if (robot is null)
        {
            return NotFound();
        }
        return Ok(robot);
    }

    [HttpGet("active")]
    public IActionResult GetActive()
    {
        return Ok(_robotService.GetActiveRobots());
    }
}
