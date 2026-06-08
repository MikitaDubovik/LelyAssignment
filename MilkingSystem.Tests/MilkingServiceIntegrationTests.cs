using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MilkingSystem.Core.Configuration;
using MilkingSystem.Core.Notifications;
using MilkingSystem.Core.Repositories;
using MilkingSystem.Core.Results;
using MilkingSystem.Core.Services;

namespace MilkingSystem.Tests;

/// <summary>
/// End-to-end integration tests for <see cref="MilkingService"/> against a real database.
/// These tests exercise the full stack: service - repository - SQL Server.
///
/// NOTE: These tests require a running database.
/// Run 'docker compose up -d' and wait ~30 s before executing.
/// </summary>
public class MilkingServiceIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly string _connectionString;
    private readonly IAnimalRepository _animalRepository;
    private readonly IRobotRepository _robotRepository;
    private readonly IMilkingEventRepository _milkingEventRepository;
    private readonly IOptions<MilkingSettings> _options;

    public MilkingServiceIntegrationTests(DatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString;
        _animalRepository = new AnimalRepository(_connectionString);
        _robotRepository = new RobotRepository(_connectionString);
        _milkingEventRepository = new MilkingEventRepository(_connectionString);
        _options = Options.Create(new MilkingSettings { ProtectionWindowHours = 6 });
    }

    /// <summary>Builds a fresh service with its own <see cref="InMemoryRobotNotifier"/> (empty cache).</summary>
    private MilkingService BuildService()
    {
        var notifier = new InMemoryRobotNotifier(
            _milkingEventRepository,
            _options,
            NullLogger<InMemoryRobotNotifier>.Instance);

        return new MilkingService(
            _animalRepository,
            _robotRepository,
            _milkingEventRepository,
            notifier,
            _options);
    }

    [Fact]
    public async Task RecordMilking_HappyPath_ReturnsSuccessAndEventIsPersistedInDB()
    {
        // Arrange
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), "Integration Cow", null);
        var robot = (await _robotRepository.GetActiveRobots()).First();
        var service = BuildService();

        // Act
        var result = await service.RecordMilking(animalId, robot.Id, 18.5m, 300, null);

        // Assert - service result
        Assert.Equal(MilkingServiceStatus.Success, result.Status);
        Assert.True(result.EventId > 0);

        // Assert - event is actually in the database (not just cached)
        var saved = await _milkingEventRepository.GetLastMilkingForAnimal(animalId);
        Assert.NotNull(saved);
        Assert.Equal(18.5m, saved!.MilkYieldLiters);
        Assert.Equal(robot.Id, saved.RobotId);
    }

    [Fact]
    public async Task RecordMilking_DoubleMilking_SecondCallRejectedWithTimestamps()
    {
        // Arrange
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), "Double Milk Cow", null);
        var robot = (await _robotRepository.GetActiveRobots()).First();
        var service = BuildService();

        // Act - first milking must succeed
        var first = await service.RecordMilking(animalId, robot.Id, 20.0m, null, null);
        Assert.Equal(MilkingServiceStatus.Success, first.Status);

        // Act - immediate second milking must be rejected
        var second = await service.RecordMilking(animalId, robot.Id, 18.0m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.RecentlyMilked, second.Status);
        Assert.NotNull(second.LastMilkedAt);
        Assert.NotNull(second.NextAllowedAt);
        Assert.True(second.NextAllowedAt > second.LastMilkedAt);
    }

    [Fact]
    public async Task RecordMilking_FreshServiceAfterFirstMilking_HydratesFromDBAndBlocksDoubleMilking()
    {
        // When the service restarts (fresh InMemoryRobotNotifier with empty cache), the first
        // WasRecentlyMilked call must trigger DB hydration and still prevent double-milking.
        // This test verifies the lazy-hydration path against real DB data.

        // Arrange - first service records a milking
        var animalId = await _animalRepository.CreateAnimal(DatabaseFixture.UniqueId(), "Hydration Test Cow", null);
        var robot = (await _robotRepository.GetActiveRobots()).First();

        var firstService = BuildService();
        var firstResult = await firstService.RecordMilking(animalId, robot.Id, 22.0m, null, null);
        Assert.Equal(MilkingServiceStatus.Success, firstResult.Status);

        // Act - completely fresh service - simulates app restart: empty in-memory state
        var freshService = BuildService();
        var secondResult = await freshService.RecordMilking(animalId, robot.Id, 20.0m, null, null);

        // Assert - fresh notifier must read from DB, find the recent event, and reject
        Assert.Equal(MilkingServiceStatus.RecentlyMilked, secondResult.Status);
    }
}
