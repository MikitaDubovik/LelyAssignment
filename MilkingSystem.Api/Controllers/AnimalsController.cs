using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly DataService _dataService;

    public AnimalsController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var animals = _dataService.GetAllAnimals();
        return Ok(animals);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var animal = _dataService.GetAnimalById(id);
        if (animal == null)
            return NotFound();
        return Ok(animal);
    }

    [HttpGet("by-identification/{identificationNumber}")]
    public IActionResult GetByIdentificationNumber(string identificationNumber)
    {
        var animal = _dataService.GetAnimalByIdentificationNumber(identificationNumber);
        if (animal == null)
            return NotFound();
        return Ok(animal);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateAnimalRequest request)
    {
        var id = _dataService.CreateAnimal(request.IdentificationNumber, request.Name, request.BirthDate);
        return Ok(new { id });
    }
}

public class CreateAnimalRequest
{
    public string IdentificationNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime? BirthDate { get; set; }
}
