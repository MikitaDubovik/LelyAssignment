using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController(DataService dataService) : ControllerBase
{
    private readonly DataService _dataService = dataService;

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
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpGet("by-identification/{identificationNumber}")]
    public IActionResult GetByIdentificationNumber(string identificationNumber)
    {
        var animal = _dataService.GetAnimalByIdentificationNumber(identificationNumber);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateAnimalRequest request)
    {
        var id = _dataService.CreateAnimal(request.IdentificationNumber, request.Name, request.BirthDate);
        return Ok(new { id });
    }
}
