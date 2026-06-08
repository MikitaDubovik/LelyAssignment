using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController(IRobotRepository robotRepository) : ControllerBase
{
    private readonly IRobotRepository _robotRepository = robotRepository;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_robotRepository.GetAllRobots());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var robot = _robotRepository.GetRobotById(id);
        if (robot is null)
        {
            return NotFound();
        }
        return Ok(robot);
    }

    [HttpGet("active")]
    public IActionResult GetActive()
    {
        return Ok(_robotRepository.GetActiveRobots());
    }
}
