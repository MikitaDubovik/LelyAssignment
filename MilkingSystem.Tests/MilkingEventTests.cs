using MilkingSystem.Core.Repositories;
using Xunit;

namespace MilkingSystem.Tests;

/// <summary>
/// Additional integration tests that demonstrate the test isolation problem.
/// These tests share database state with DataServiceIntegrationTests.
/// </summary>
public class MilkingEventTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly IAnimalRepository _animalRepository;
    private readonly IMilkingEventRepository _milkingEventRepository;

    public MilkingEventTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _animalRepository = new AnimalRepository(_fixture.ConnectionString);
        _milkingEventRepository = new MilkingEventRepository(_fixture.ConnectionString);
    }

    [Fact]
    public async Task GetRecentMilkingEvents_WithNoRecentEvents_ReturnsEmptyList()
    {
        // This test is FLAKY because it assumes no milking events in the last hour
        // But other tests may have inserted events that affect this result

        // Act
        var events = await _milkingEventRepository.GetRecentMilkingEvents(hours: 1);

        // Assert
        // This might pass or fail depending on when other tests ran
        // and whether they inserted events within the last hour

        // INTENTIONALLY FLAKY: Sometimes there will be recent events, sometimes not
        // depending on test execution order and timing
        Assert.NotNull(events);
    }

    [Fact]
    public async Task CreateAnimal_WithDuplicateIdentificationNumber_ShouldFail()
    {
        // Arrange - uses same static counter as other tests
        var identificationNumber = $"TEST-{TestDataHelper.GetNextAnimalId()}";

        // First creation should succeed
        var firstId = await _animalRepository.CreateAnimal(identificationNumber, "First Animal", null);
        Assert.True(firstId > 0);

        // Second creation with same ID should throw
        // Note: This creates test data pollution
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _animalRepository.CreateAnimal(identificationNumber, "Second Animal", null));
    }

    [Fact]
    public async Task GetLastMilkingForAnimal_WhenNoMilkings_ReturnsNull()
    {
        // Arrange - create a brand new animal that has no milkings
        var identificationNumber = $"NOMILK-{TestDataHelper.GetNextAnimalId()}";
        var animalId = await _animalRepository.CreateAnimal(identificationNumber, "No Milking Animal", null);

        // Act
        var lastMilking = await _milkingEventRepository.GetLastMilkingForAnimal(animalId);

        // Assert
        Assert.Null(lastMilking);

        // NOTE: This animal is left in the database after the test!
    }
}
