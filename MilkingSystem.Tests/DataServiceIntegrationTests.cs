using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Tests;

/// <summary>
/// Integration tests for the repository layer.
///
/// NOTE: These tests require a running database.
/// Run 'docker compose up -d' and wait ~30 s before executing.
/// </summary>
public class DataServiceIntegrationTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    private readonly IAnimalRepository _animalRepository = new AnimalRepository(fixture.ConnectionString);
    private readonly IRobotRepository _robotRepository = new RobotRepository(fixture.ConnectionString);
    private readonly IMilkingEventRepository _milkingEventRepository = new MilkingEventRepository(fixture.ConnectionString);



    [Fact]
    public async Task GetAllAnimals_ReturnsSeededAnimals()
    {
        // Act
        var animals = await _animalRepository.GetAllAnimals();

        // Assert
        Assert.NotNull(animals);
        Assert.True(animals.Count > 0, "Expected at least one animal from seed data");
    }

    [Fact]
    public async Task GetAnimalById_WithValidId_ReturnsAnimal()
    {
        // Arrange
        var animals = await _animalRepository.GetAllAnimals();
        var firstAnimal = animals.First();

        // Act
        var animal = await _animalRepository.GetAnimalById(firstAnimal.Id);

        // Assert
        Assert.NotNull(animal);
        Assert.Equal(firstAnimal.Id, animal!.Id);
    }

    [Fact]
    public async Task GetAnimalById_WithInvalidId_ReturnsNull()
    {
        // Act
        var animal = await _animalRepository.GetAnimalById(-1);

        // Assert
        Assert.Null(animal);
    }

    [Fact]
    public async Task CreateAnimal_PersistsAndCanBeRetrievedById()
    {
        // Arrange — Guid-based ID avoids collisions across repeated test runs
        var identificationNumber = DatabaseFixture.UniqueId();

        // Act
        var id = await _animalRepository.CreateAnimal(identificationNumber, "Test Animal", DateTime.UtcNow.AddYears(-2));

        // Assert
        Assert.True(id > 0);

        var animal = await _animalRepository.GetAnimalById(id);
        Assert.NotNull(animal);
        Assert.Equal(identificationNumber, animal!.IdentificationNumber);
    }

    [Fact]
    public async Task SaveMilkingEvent_PersistsAndReturnsGeneratedId()
    {
        // Arrange — create a fresh animal so this test is fully self-contained
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), null, null);
        var robots = await _robotRepository.GetAllRobots();
        var robot = robots.First();

        // Act
        var id = await _milkingEventRepository.SaveMilkingEvent(
            animalId, robot.Id, DateTime.UtcNow, 25.5m, 360);

        // Assert
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetMilkingEventsForAnimal_ReturnsSavedEvents()
    {
        // Arrange — create animal and event so this test does not depend on other tests
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), null, null);
        var robots = await _robotRepository.GetAllRobots();
        await _milkingEventRepository.SaveMilkingEvent(
            animalId, robots.First().Id, DateTime.UtcNow, 22.0m, null);

        // Act
        var events = await _milkingEventRepository.GetMilkingEventsForAnimal(animalId);

        // Assert
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.Equal(22.0m, events[0].MilkYieldLiters);
    }
}
