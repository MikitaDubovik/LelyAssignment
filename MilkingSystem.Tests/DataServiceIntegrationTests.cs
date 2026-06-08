using MilkingSystem.Core.Services;
using Xunit;

namespace MilkingSystem.Tests;

/// <summary>
/// Integration tests for DataService.
/// 
/// NOTE: These tests require a running database. 
/// Run 'docker-compose up' before executing these tests.
/// 
/// WARNING: There may be issues with test isolation in this class.
/// </summary>
public class DataServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly DataService _dataService;

    public DataServiceIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _dataService = new DataService(_fixture.ConnectionString);
    }

    [Fact]
    public void GetAllAnimals_ReturnsAnimals()
    {
        // Act
        var animals = _dataService.GetAllAnimals();

        // Assert
        Assert.NotNull(animals);
        Assert.True(animals.Count > 0, "Expected at least one animal from seed data");
    }

    [Fact]
    public void GetAnimalById_WithValidId_ReturnsAnimal()
    {
        // Arrange
        var animals = _dataService.GetAllAnimals();
        var firstAnimal = animals.First();

        // Act
        var animal = _dataService.GetAnimalById(firstAnimal.Id);

        // Assert
        Assert.NotNull(animal);
        Assert.Equal(firstAnimal.Id, animal!.Id);
    }

    [Fact]
    public void GetAnimalById_WithInvalidId_ReturnsNull()
    {
        // Act
        var animal = _dataService.GetAnimalById(99999);

        // Assert
        Assert.Null(animal);
    }

    [Fact]
    public void CreateAnimal_CreatesNewAnimal()
    {
        // Arrange - using static counter that persists across test runs
        var identificationNumber = $"TEST-{TestDataHelper.GetNextAnimalId()}";

        // Act
        var id = _dataService.CreateAnimal(identificationNumber, "Test Animal", DateTime.Now.AddYears(-2));

        // Assert
        Assert.True(id > 0);
        
        var animal = _dataService.GetAnimalById(id);
        Assert.NotNull(animal);
        Assert.Equal(identificationNumber, animal!.IdentificationNumber);
    }

    [Fact]
    public void SaveMilkingEvent_SavesEvent()
    {
        // Arrange
        var animals = _dataService.GetAllAnimals();
        var animal = animals.First();
        var robots = _dataService.GetAllRobots();
        var robot = robots.First();

        // Act
        var id = _dataService.SaveMilkingEvent(
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
    public void GetMilkingEventsForAnimal_ReturnsEvents()
    {
        // Arrange - This test depends on SaveMilkingEvent_SavesEvent having run first
        // and may fail if run in isolation or in different order
        var animals = _dataService.GetAllAnimals();
        var animal = animals.First();

        // Act
        var events = _dataService.GetMilkingEventsForAnimal(animal.Id);

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
