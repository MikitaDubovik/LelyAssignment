using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Tests;

/// <summary>
/// Integration tests for milking-event specific repository behaviour.
///
/// NOTE: These tests require a running database.
/// Run 'docker compose up -d' and wait ~30 s before executing.
/// </summary>
public class MilkingEventTests(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    private readonly IAnimalRepository _animalRepository = new AnimalRepository(fixture.ConnectionString);
    private readonly IMilkingEventRepository _milkingEventRepository = new MilkingEventRepository(fixture.ConnectionString);


    [Fact]
    public async Task CreateAnimal_WithDuplicateIdentificationNumber_Throws()
    {
        // Arrange - share the same number for both inserts
        var identificationNumber = DatabaseFixture.UniqueId();
        await _animalRepository.CreateAnimal(identificationNumber, "First", null);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() =>
            _animalRepository.CreateAnimal(identificationNumber, "Duplicate", null));
    }

    [Fact]
    public async Task GetLastMilkingForAnimal_WhenNoMilkings_ReturnsNull()
    {
        // Arrange - brand-new animal with no history
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), null, null);

        // Act
        var last = await _milkingEventRepository.GetLastMilkingForAnimal(animalId);

        // Assert
        Assert.Null(last);
    }

    [Fact]
    public async Task GetLastMilkingForAnimal_AfterSavingEvent_ReturnsIt()
    {
        // Arrange
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), null, null);
        var savedId = await _milkingEventRepository.SaveMilkingEvent(animalId, 1, DateTime.UtcNow, 19.5m, null);

        // Act
        var last = await _milkingEventRepository.GetLastMilkingForAnimal(animalId);

        // Assert
        Assert.NotNull(last);
        Assert.Equal(savedId, last!.Id);
        Assert.Equal(19.5m, last.MilkYieldLiters);
    }

    [Fact]
    public async Task GetRecentMilkingEvents_EventSavedNow_AppearsInResults()
    {
        // Arrange
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), null, null);
        await _milkingEventRepository.SaveMilkingEvent(animalId, 1, DateTime.UtcNow, 21.0m, null);

        // Act
        var recent = await _milkingEventRepository.GetRecentMilkingEvents(hours: 1);

        // Assert
        Assert.Contains(recent, e => e.AnimalId == animalId);
    }

    [Fact]
    public async Task GetRecentMilkingEvents_EventSavedTwoHoursAgo_DoesNotAppearInOneHourWindow()
    {
        // Arrange
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), null, null);
        await _milkingEventRepository.SaveMilkingEvent(
            animalId, 1, DateTime.UtcNow.AddHours(-2), 21.0m, null);

        // Act
        var recent = await _milkingEventRepository.GetRecentMilkingEvents(hours: 1);

        // Assert - the old event must not appear in a 1-hour window
        Assert.DoesNotContain(recent, e => e.AnimalId == animalId);
    }
}
