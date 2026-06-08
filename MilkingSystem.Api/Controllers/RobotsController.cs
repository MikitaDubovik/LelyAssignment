using Autofac;
using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RobotsController : ControllerBase
{
    private readonly ILifetimeScope _scope;

    public RobotsController(ILifetimeScope scope)
    {
        _scope = scope;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var dataService = _scope.Resolve<DataService>();
        var robots = dataService.GetAllRobots();
        return Ok(robots);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var dataService = _scope.Resolve<DataService>();
        var robot = dataService.GetRobotById(id);
        
        if (robot == null)
            return NotFound();
        
        return Ok(robot);
    }

    [HttpGet("active")]
    public IActionResult GetActive()
    {
        var dataService = _scope.Resolve<DataService>();
        var robots = dataService.GetActiveRobots();
        return Ok(robots);
    }
}
