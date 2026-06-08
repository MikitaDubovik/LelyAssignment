using MilkingSystem.Core.Repositories;
using Xunit;

namespace MilkingSystem.Tests;

/// <summary>
/// Integration tests for the repository layer.
///
/// NOTE: These tests require a running database.
/// Run 'docker-compose up' before executing these tests.
///
/// WARNING: There may be issues with test isolation in this class.
/// </summary>
public class DataServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly IAnimalRepository _animalRepository;
    private readonly IRobotRepository _robotRepository;
    private readonly IMilkingEventRepository _milkingEventRepository;

    public DataServiceIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _animalRepository = new AnimalRepository(_fixture.ConnectionString);
        _robotRepository = new RobotRepository(_fixture.ConnectionString);
        _milkingEventRepository = new MilkingEventRepository(_fixture.ConnectionString);
    }

    [Fact]
    public async Task GetAllAnimals_ReturnsAnimals()
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
        var animal = await _animalRepository.GetAnimalById(99999);

        // Assert
        Assert.Null(animal);
    }

    [Fact]
    public async Task CreateAnimal_CreatesNewAnimal()
    {
        // Arrange - using static counter that persists across test runs
        var identificationNumber = $"TEST-{TestDataHelper.GetNextAnimalId()}";

        // Act
        var id = await _animalRepository.CreateAnimal(identificationNumber, "Test Animal", DateTime.Now.AddYears(-2));

        // Assert
        Assert.True(id > 0);

        var animal = await _animalRepository.GetAnimalById(id);
        Assert.NotNull(animal);
        Assert.Equal(identificationNumber, animal!.IdentificationNumber);
    }

    [Fact]
    public async Task SaveMilkingEvent_SavesEvent()
    {
        // Arrange
        var animals = await _animalRepository.GetAllAnimals();
        var animal = animals.First();
        var robots = await _robotRepository.GetAllRobots();
        var robot = robots.First();

        // Act
        var id = await _milkingEventRepository.SaveMilkingEvent(
            animal.Id,
            robot.Id,
            DateTime.UtcNow,
            25.5m,
            360
        );

        // Assert
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetMilkingEventsForAnimal_ReturnsEvents()
    {
        // Arrange - This test depends on SaveMilkingEvent_SavesEvent having run first
        // and may fail if run in isolation or in different order
        var animals = await _animalRepository.GetAllAnimals();
        var animal = animals.First();

        // Act
        var events = await _milkingEventRepository.GetMilkingEventsForAnimal(animal.Id);

        // Assert
        Assert.NotNull(events);
        // This assertion is FLAKY - it assumes previous test data exists
        Assert.True(events.Count > 0, "Expected milking events for animal");
    }
}

/// <summary>
/// Shared test data helper - WARNING: Uses static state!
/// </summary>
public static class TestDataHelper
{
    // Static counter - this causes test pollution between test runs
    private static int _animalCounter = 1000;

    public static int GetNextAnimalId()
    {
        return _animalCounter++;
    }

    // This doesn't get reset between test classes or test runs!
    public static void Reset()
    {
        _animalCounter = 1000;
    }
}
