using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController(IRobotService robotService) : ControllerBase
{
    private readonly IRobotService _robotService = robotService;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _robotService.GetAllRobots());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var robot = await _robotService.GetRobotById(id);
        if (robot is null)
        {
            return NotFound();
        }
        return Ok(robot);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        return Ok(await _robotService.GetActiveRobots());
    }
}
