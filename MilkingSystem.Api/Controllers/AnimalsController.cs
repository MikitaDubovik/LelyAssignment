using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController(IAnimalRepository animalRepository) : ControllerBase
{
    private readonly IAnimalRepository _animalRepository = animalRepository;

    [HttpGet]
    public IActionResult Get()
    {
        var animals = _animalRepository.GetAllAnimals();
        return Ok(animals);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var animal = _animalRepository.GetAnimalById(id);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpGet("by-identification/{identificationNumber}")]
    public IActionResult GetByIdentificationNumber(string identificationNumber)
    {
        var animal = _animalRepository.GetAnimalByIdentificationNumber(identificationNumber);
        if (animal is null)
        {
            return NotFound();
        }
        return Ok(animal);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateAnimalRequest request)
    {
        var id = _animalRepository.CreateAnimal(request.IdentificationNumber, request.Name, request.BirthDate);
        return Ok(new { id });
    }
}
