using Microsoft.Extensions.Options;
using MilkingSystem.Core.Configuration;
using MilkingSystem.Core.Models;
using MilkingSystem.Core.Notifications;
using MilkingSystem.Core.Repositories;
using MilkingSystem.Core.Results;
using MilkingSystem.Core.Services;
using Moq;

namespace MilkingSystem.Tests;

public class MilkingServiceTests
{
    [Fact]
    public async Task RecordMilking_ValidRequest_ReturnsSuccessWithEventId()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync((MilkingEvent?)null);
        milkingRepo
            .Setup(r => r.SaveMilkingEvent(1, 1, It.IsAny<DateTime>(), 25.5m, 420))
            .ReturnsAsync(42);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        var result = await service.RecordMilking(1, 1, 25.5m, 420, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.Success, result.Status);
        Assert.Equal(42, result.EventId);
    }

    [Fact]
    public async Task RecordMilking_WithExplicitTimestamp_UsesProvidedTimestampNotUtcNow()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        var explicitTime = new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync((MilkingEvent?)null);
        milkingRepo
            .Setup(r => r.SaveMilkingEvent(1, 1, explicitTime, 25.5m, null))
            .ReturnsAsync(1);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        await service.RecordMilking(1, 1, 25.5m, null, explicitTime);

        // Assert — the exact provided timestamp must reach the repository unchanged
        milkingRepo.Verify(r => r.SaveMilkingEvent(1, 1, explicitTime, 25.5m, null), Times.Once);
    }

    [Fact]
    public async Task RecordMilking_Success_NotifiesWithCorrectAnimalAndRobotIds()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal(1));
        robotRepo.Setup(r => r.GetRobotById(2)).ReturnsAsync(MakeRobot(2));
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync((MilkingEvent?)null);
        milkingRepo
            .Setup(r => r.SaveMilkingEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<int?>()))
            .ReturnsAsync(1);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        await service.RecordMilking(1, 2, 25.5m, null, null);

        // Assert
        notifier.Verify(
            n => n.NotifyMilkingCompleted(It.Is<MilkingNotification>(m => m.AnimalId == 1 && m.RobotId == 2)),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task RecordMilking_NonPositiveMilkYield_ReturnsMilkAmountIsIncorrect(decimal yield)
    {
        // The service has its own guard independent of model validation.
        var service = Build();

        var result = await service.RecordMilking(1, 1, yield, null, null);

        Assert.Equal(MilkingServiceStatus.MilkAmountIsIncorrect, result.Status);
    }

    [Fact]
    public async Task RecordMilking_AnimalNotFound_ReturnsAnimalNotFound()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        animalRepo.Setup(r => r.GetAnimalById(It.IsAny<int>())).ReturnsAsync((Animal?)null);

        var service = Build(animalRepo: animalRepo);

        // Act
        var result = await service.RecordMilking(99, 1, 25.5m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.AnimalNotFound, result.Status);
    }

    [Fact]
    public async Task RecordMilking_RobotNotFound_ReturnsRobotNotFound()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(It.IsAny<int>())).ReturnsAsync((Robot?)null);

        var service = Build(animalRepo: animalRepo, robotRepo: robotRepo);

        // Act
        var result = await service.RecordMilking(1, 99, 25.5m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.RobotNotFound, result.Status);
    }

    [Fact]
    public async Task RecordMilking_RobotInactive_ReturnsRobotNotActive()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot(isActive: false));

        var service = Build(animalRepo: animalRepo, robotRepo: robotRepo);

        // Act
        var result = await service.RecordMilking(1, 1, 25.5m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.RobotNotActive, result.Status);
    }

    [Fact]
    public async Task RecordMilking_PreCheckFires_ReturnsRecentlyMilkedWithTimestamps()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        var lastEvent = MakeMilkingEvent(hoursAgo: 2);

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(true);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync(lastEvent);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        var result = await service.RecordMilking(1, 1, 25.5m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.RecentlyMilked, result.Status);
        Assert.Equal(lastEvent.Timestamp, result.LastMilkedAt);
        Assert.Equal(lastEvent.Timestamp.AddHours(6), result.NextAllowedAt);
    }

    [Fact]
    public async Task RecordMilking_PreCheckFires_DoesNotSaveOrNotify()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(true);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync(MakeMilkingEvent());

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        await service.RecordMilking(1, 1, 25.5m, null, null);

        // Assert
        milkingRepo.Verify(
            r => r.SaveMilkingEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<int?>()),
            Times.Never);
        notifier.Verify(n => n.NotifyMilkingCompleted(It.IsAny<MilkingNotification>()), Times.Never);
    }

    [Fact]
    public async Task RecordMilking_DbCheckFires_ReturnsRecentlyMilkedWithTimestamps()
    {
        // Pre-check says "not recently milked" but DB reveals a concurrent request already saved.
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        var lastEvent = MakeMilkingEvent(hoursAgo: 1);

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync(lastEvent);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        var result = await service.RecordMilking(1, 1, 25.5m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.RecentlyMilked, result.Status);
        Assert.Equal(lastEvent.Timestamp, result.LastMilkedAt);
        Assert.Equal(lastEvent.Timestamp.AddHours(6), result.NextAllowedAt);
    }

    [Fact]
    public async Task RecordMilking_DbCheckFires_DoesNotSaveOrNotify()
    {
        // Arrange
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync(MakeMilkingEvent(hoursAgo: 1));

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        await service.RecordMilking(1, 1, 25.5m, null, null);

        // Assert
        milkingRepo.Verify(
            r => r.SaveMilkingEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<int?>()),
            Times.Never);
        notifier.Verify(n => n.NotifyMilkingCompleted(It.IsAny<MilkingNotification>()), Times.Never);
    }

    [Fact]
    public async Task RecordMilking_LastMilkingOutsideProtectionWindow_Allowed()
    {
        // An event 7 hours ago should not block a new milking with a 6-hour window.
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync(MakeMilkingEvent(hoursAgo: 7));
        milkingRepo
            .Setup(r => r.SaveMilkingEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<int?>()))
            .ReturnsAsync(10);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        var result = await service.RecordMilking(1, 1, 25.5m, null, null);

        // Assert
        Assert.Equal(MilkingServiceStatus.Success, result.Status);
    }

    [Fact]
    public async Task RecordMilking_TwoConcurrentCallsSameAnimal_ExactlyOneSucceeds()
    {
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        var saved = false;
        var savedAt = DateTime.UtcNow;

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal());
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot());
        notifier.Setup(n => n.WasRecentlyMilked(1, 6)).ReturnsAsync(false);

        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1))
            .ReturnsAsync(() => saved
                ? new MilkingEvent { AnimalId = 1, RobotId = 1, Timestamp = savedAt, MilkYieldLiters = 25.5m }
                : null);

        milkingRepo
            .Setup(r => r.SaveMilkingEvent(1, 1, It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<int?>()))
            .ReturnsAsync(() => { savedAt = DateTime.UtcNow; saved = true; return 1; });

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act — both tasks are started before either is awaited; they compete for the per-animal lock
        var t1 = service.RecordMilking(1, 1, 25.5m, null, null);
        var t2 = service.RecordMilking(1, 1, 25.5m, null, null);
        var results = await Task.WhenAll(t1, t2);

        // Assert
        Assert.Equal(1, results.Count(r => r.Status == MilkingServiceStatus.Success));
        Assert.Equal(1, results.Count(r => r.Status == MilkingServiceStatus.RecentlyMilked));
    }

    [Fact]
    public async Task RecordMilking_TwoConcurrentCallsDifferentAnimals_BothSucceed()
    {
        // Different animals use independent locks and must not block each other.
        var animalRepo = new Mock<IAnimalRepository>();
        var robotRepo = new Mock<IRobotRepository>();
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var notifier = new Mock<IRobotNotifier>();

        animalRepo.Setup(r => r.GetAnimalById(1)).ReturnsAsync(MakeAnimal(1));
        animalRepo.Setup(r => r.GetAnimalById(2)).ReturnsAsync(MakeAnimal(2));
        robotRepo.Setup(r => r.GetRobotById(1)).ReturnsAsync(MakeRobot(1));
        robotRepo.Setup(r => r.GetRobotById(2)).ReturnsAsync(MakeRobot(2));
        notifier.Setup(n => n.WasRecentlyMilked(It.IsAny<int>(), 6)).ReturnsAsync(false);
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(It.IsAny<int>())).ReturnsAsync((MilkingEvent?)null);
        milkingRepo
            .Setup(r => r.SaveMilkingEvent(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<int?>()))
            .ReturnsAsync(1);

        var service = Build(animalRepo, robotRepo, milkingRepo, notifier);

        // Act
        var t1 = service.RecordMilking(1, 1, 25.5m, null, null);
        var t2 = service.RecordMilking(2, 2, 25.5m, null, null);
        var results = await Task.WhenAll(t1, t2);

        // Assert
        Assert.All(results, r => Assert.Equal(MilkingServiceStatus.Success, r.Status));
    }

    [Fact]
    public async Task GetMilkingEventsForAnimal_DelegatesToRepository()
    {
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var expected = new List<MilkingEvent> { MakeMilkingEvent() };
        milkingRepo.Setup(r => r.GetMilkingEventsForAnimal(1)).ReturnsAsync(expected);

        var result = await Build(milkingRepo: milkingRepo).GetMilkingEventsForAnimal(1);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetLastMilkingForAnimal_DelegatesToRepository()
    {
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var expected = MakeMilkingEvent();
        milkingRepo.Setup(r => r.GetLastMilkingForAnimal(1)).ReturnsAsync(expected);

        var result = await Build(milkingRepo: milkingRepo).GetLastMilkingForAnimal(1);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetRecentMilkingEvents_DelegatesToRepository()
    {
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var expected = new List<MilkingEvent> { MakeMilkingEvent() };
        milkingRepo.Setup(r => r.GetRecentMilkingEvents(12)).ReturnsAsync(expected);

        var result = await Build(milkingRepo: milkingRepo).GetRecentMilkingEvents(12);

        Assert.Same(expected, result);
    }

    private static Animal MakeAnimal(int id = 1) =>
        new() { Id = id, IdentificationNumber = $"TAG-{id}" };

    private static Robot MakeRobot(int id = 1, bool isActive = true) =>
        new() { Id = id, Name = $"Robot-{id}", IsActive = isActive };

    private static MilkingEvent MakeMilkingEvent(int animalId = 1, double hoursAgo = 1) =>
        new()
        {
            Id = 99,
            AnimalId = animalId,
            RobotId = 1,
            Timestamp = DateTime.UtcNow.AddHours(-hoursAgo),
            MilkYieldLiters = 20m
        };

    private static IOptions<MilkingSettings> MakeOptions(int protectionWindowHours = 6) =>
        Options.Create(new MilkingSettings { ProtectionWindowHours = protectionWindowHours });

    private static MilkingService Build(
        Mock<IAnimalRepository>? animalRepo = null,
        Mock<IRobotRepository>? robotRepo = null,
        Mock<IMilkingEventRepository>? milkingRepo = null,
        Mock<IRobotNotifier>? notifier = null,
        int protectionWindowHours = 6) =>
        new(
            (animalRepo ?? new Mock<IAnimalRepository>()).Object,
            (robotRepo ?? new Mock<IRobotRepository>()).Object,
            (milkingRepo ?? new Mock<IMilkingEventRepository>()).Object,
            (notifier ?? new Mock<IRobotNotifier>()).Object,
            MakeOptions(protectionWindowHours));
}
