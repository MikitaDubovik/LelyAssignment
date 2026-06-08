using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController(IAnimalService animalService) : ControllerBase
{
    private readonly IAnimalService _animalService = animalService;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_animalService.GetAllAnimals());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var animal = _animalService.GetAnimalById(id);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpGet("by-identification/{identificationNumber}")]
    public IActionResult GetByIdentificationNumber(string identificationNumber)
    {
        var animal = _animalService.GetAnimalByIdentificationNumber(identificationNumber);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateAnimalRequest request)
    {
        var id = _animalService.CreateAnimal(request.IdentificationNumber, request.Name, request.BirthDate);
        return Ok(new { id });
    }
}
