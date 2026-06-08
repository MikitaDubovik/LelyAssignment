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
    public async Task<IActionResult> Get()
    {
        return Ok(await _animalService.GetAllAnimals());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var animal = await _animalService.GetAnimalById(id);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpGet("by-identification/{identificationNumber}")]
    public async Task<IActionResult> GetByIdentificationNumber(string identificationNumber)
    {
        var animal = await _animalService.GetAnimalByIdentificationNumber(identificationNumber);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnimalRequest request)
    {
        var id = await _animalService.CreateAnimal(request.IdentificationNumber, request.Name, request.BirthDate);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }
}
