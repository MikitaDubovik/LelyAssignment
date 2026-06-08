using System.Data.SqlClient;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Core.Models;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly ILifetimeScope _scope;
    private readonly IConfiguration _configuration;

    public AnimalsController(ILifetimeScope scope, IConfiguration configuration)
    {
        _scope = scope;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var animals = new List<Animal>();
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Animals ORDER BY Name", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animals.Add(new Animal
                {
                    Id = (int)reader["Id"],
                    IdentificationNumber = reader["IdentificationNumber"].ToString()!,
                    Name = reader["Name"] as string,
                    BirthDate = reader["BirthDate"] as DateTime?,
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                });
            }
        }
        
        return Ok(animals);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var dataService = _scope.Resolve<DataService>();
        var animal = dataService.GetAnimalById(id);
        
        if (animal == null)
            return NotFound();
        
        return Ok(animal);
    }

    [HttpGet("by-identification/{identificationNumber}")]
    public IActionResult GetByIdentificationNumber(string identificationNumber)
    {
        try
        {
            var dataService = _scope.Resolve<DataService>();
            var animal = dataService.GetAnimalByIdentificationNumber(identificationNumber);
            
            if (animal == null)
                return NotFound();
            
            return Ok(animal);
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateAnimalRequest request)
    {
        var dataService = _scope.Resolve<DataService>();
        
        var id = dataService.CreateAnimal(request.IdentificationNumber, request.Name, request.BirthDate);
        
        return Ok(new { id });
    }
}

public class CreateAnimalRequest
{
    public string IdentificationNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime? BirthDate { get; set; }
}
