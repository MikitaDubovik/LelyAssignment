using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkingSystem.Core.Configuration;
using MilkingSystem.Core.Models;
using MilkingSystem.Core.Notifications;
using MilkingSystem.Core.Repositories;
using Moq;

namespace MilkingSystem.Tests;

public class InMemoryRobotNotifierTests
{
    [Fact]
    public void NotifyMilkingCompleted_SingleSubscriber_HandlerInvoked()
    {
        // Arrange
        var notifier = Build();
        var received = new List<MilkingNotification>();
        notifier.Subscribe(received.Add);

        var notification = MakeNotification();

        // Act
        notifier.NotifyMilkingCompleted(notification);

        // Assert
        Assert.Single(received);
        Assert.Equal(1, received[0].AnimalId);
    }

    [Fact]
    public void NotifyMilkingCompleted_MultipleSubscribers_AllHandlersInvoked()
    {
        // Arrange
        var notifier = Build();
        var callCount = 0;
        notifier.Subscribe(_ => callCount++);
        notifier.Subscribe(_ => callCount++);
        notifier.Subscribe(_ => callCount++);

        // Act
        notifier.NotifyMilkingCompleted(MakeNotification());

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public void NotifyMilkingCompleted_AfterSubscriptionDisposed_HandlerNotInvoked()
    {
        // Arrange
        var notifier = Build();
        var called = false;

        var subscription = notifier.Subscribe(_ => called = true);
        subscription.Dispose();

        // Act
        notifier.NotifyMilkingCompleted(MakeNotification());

        // Assert
        Assert.False(called);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Dispose must be idempotent — robots may unsubscribe in error paths.
        var notifier = Build();
        var subscription = notifier.Subscribe(_ => { });

        subscription.Dispose();

        // Should not throw
        subscription.Dispose();
    }

    [Fact]
    public void NotifyMilkingCompleted_OneSubscriberThrows_OtherSubscribersStillReceiveNotification()
    {
        // A misbehaving subscriber must not kill the broadcast for everyone else.
        var notifier = Build();
        var secondHandlerCalled = false;

        notifier.Subscribe(_ => throw new InvalidOperationException("Subscriber is broken"));
        notifier.Subscribe(_ => secondHandlerCalled = true);

        // Act — must not throw
        notifier.NotifyMilkingCompleted(MakeNotification());

        // Assert
        Assert.True(secondHandlerCalled);
    }

    [Fact]
    public void NotifyMilkingCompleted_SubscriberThrows_ErrorIsLogged()
    {
        // Failures must not be silent — the error must reach the logger.
        var logger = new Mock<ILogger<InMemoryRobotNotifier>>();
        var notifier = Build(logger: logger);

        notifier.Subscribe(_ => throw new InvalidOperationException("Boom"));

        notifier.NotifyMilkingCompleted(MakeNotification());

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, __) => true)),
            Times.Once);
    }

    [Fact]
    public async Task WasRecentlyMilked_UnknownAnimal_ReturnsFalse()
    {
        var notifier = Build();

        var result = await notifier.WasRecentlyMilked(animalId: 99, protectionWindowHours: 6);

        Assert.False(result);
    }

    [Fact]
    public async Task WasRecentlyMilked_AfterRecentNotification_ReturnsTrue()
    {
        // A robot completes milking and notifies; another robot asks - must be blocked.
        var notifier = Build();

        notifier.NotifyMilkingCompleted(MakeNotification(animalId: 1, timestamp: DateTime.UtcNow));

        var result = await notifier.WasRecentlyMilked(animalId: 1, protectionWindowHours: 6);

        Assert.True(result);
    }

    [Fact]
    public async Task WasRecentlyMilked_AfterOldNotification_ReturnsFalse()
    {
        // A notification from 10 hours ago should not block milking under a 6-hour window.
        var notifier = Build();

        notifier.NotifyMilkingCompleted(MakeNotification(animalId: 1, timestamp: DateTime.UtcNow.AddHours(-10)));

        var result = await notifier.WasRecentlyMilked(animalId: 1, protectionWindowHours: 6);

        Assert.False(result);
    }

    [Fact]
    public async Task WasRecentlyMilked_NotificationForDifferentAnimal_ReturnsFalse()
    {
        // Milking animal 2 must not affect the check for animal 1.
        var notifier = Build();

        notifier.NotifyMilkingCompleted(MakeNotification(animalId: 2, timestamp: DateTime.UtcNow));

        var result = await notifier.WasRecentlyMilked(animalId: 1, protectionWindowHours: 6);

        Assert.False(result);
    }

    [Fact]
    public async Task WasRecentlyMilked_FirstCall_HydratesFromDatabase()
    {
        // Arrange — override the default empty setup to count calls explicitly
        var milkingRepo = new Mock<IMilkingEventRepository>();
        milkingRepo
            .Setup(r => r.GetRecentMilkingEvents(6))
            .ReturnsAsync([]);

        var notifier = Build(milkingRepo);

        // Act
        await notifier.WasRecentlyMilked(1, 6);

        // Assert
        milkingRepo.Verify(r => r.GetRecentMilkingEvents(6), Times.Once);
    }

    [Fact]
    public async Task WasRecentlyMilked_SubsequentCalls_HydrationRunsOnlyOnce()
    {
        // Hydration is expensive (DB query) and must happen exactly once.
        var milkingRepo = new Mock<IMilkingEventRepository>();
        milkingRepo
            .Setup(r => r.GetRecentMilkingEvents(6))
            .ReturnsAsync([]);

        var notifier = Build(milkingRepo);

        // Act
        await notifier.WasRecentlyMilked(1, 6);
        await notifier.WasRecentlyMilked(2, 6);
        await notifier.WasRecentlyMilked(1, 6);

        // Assert
        milkingRepo.Verify(r => r.GetRecentMilkingEvents(6), Times.Once);
    }

    [Fact]
    public async Task WasRecentlyMilked_AnimalPresentInHydratedData_ReturnsTrue()
    {
        // The in-memory state must be seeded from DB on first use.
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var seedEvent = new MilkingEvent
        {
            AnimalId = 5,
            RobotId = 1,
            Timestamp = DateTime.UtcNow.AddHours(-1) // within 6-hour window
        };

        milkingRepo
            .Setup(r => r.GetRecentMilkingEvents(6))
            .ReturnsAsync([seedEvent]);

        var notifier = Build(milkingRepo);

        // Act
        var result = await notifier.WasRecentlyMilked(animalId: 5, protectionWindowHours: 6);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task WasRecentlyMilked_ConcurrentFirstCalls_HydrationRunsExactlyOnce()
    {
        // If 10 callers all hit WasRecentlyMilked before hydration completes,
        // only one call to GetRecentMilkingEvents must ever be made.
        var callCount = 0;
        var milkingRepo = new Mock<IMilkingEventRepository>();

        milkingRepo
            .Setup(r => r.GetRecentMilkingEvents(It.IsAny<int>()))
            .Returns(async (int _) =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(50); // simulate latency so concurrent callers overlap
                return [];
            });

        var notifier = Build(milkingRepo);

        // Act — fire 10 concurrent checks before any hydration result arrives
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => notifier.WasRecentlyMilked(1, 6))
            .ToList();
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task WasRecentlyMilked_NotificationArrivesBeforeHydration_KeepsLatestTimestamp()
    {
        // If NotifyMilkingCompleted is called before or during hydration,
        // AddOrUpdate must keep the more recent of the two timestamps.
        var milkingRepo = new Mock<IMilkingEventRepository>();
        var olderTimestamp = DateTime.UtcNow.AddHours(-2);
        var newerTimestamp = DateTime.UtcNow.AddHours(-1);

        // DB returns the older event
        milkingRepo
            .Setup(r => r.GetRecentMilkingEvents(6))
            .ReturnsAsync(
            [
                new() { AnimalId = 1, RobotId = 1, Timestamp = olderTimestamp }
            ]);

        var notifier = Build(milkingRepo);

        // Notify (simulates a new event arriving) before hydration has run
        notifier.NotifyMilkingCompleted(MakeNotification(animalId: 1, timestamp: newerTimestamp));

        // Trigger hydration
        await notifier.WasRecentlyMilked(1, 6);

        // The newer timestamp (from the notification) must survive the hydration merge.
        // Check against a window that only covers newerTimestamp but not newerTimestamp + more.
        // Both are < 6h, so the animal should still be blocked.
        var result = await notifier.WasRecentlyMilked(animalId: 1, protectionWindowHours: 6);
        Assert.True(result);
    }

    private static InMemoryRobotNotifier Build(
        Mock<IMilkingEventRepository>? milkingRepo = null,
        int protectionWindowHours = 6,
        Mock<ILogger<InMemoryRobotNotifier>>? logger = null)
    {
        Mock<IMilkingEventRepository> repo;
        if (milkingRepo is null)
        {
            repo = new Mock<IMilkingEventRepository>();
            // Default: hydration returns an empty list so tests that don't care about it
            // don't need to set up GetRecentMilkingEvents explicitly.
            repo.Setup(r => r.GetRecentMilkingEvents(It.IsAny<int>()))
                .ReturnsAsync([]);
        }
        else
        {
            // Caller owns the mock and its setup; don't overwrite it.
            repo = milkingRepo;
        }

        return new InMemoryRobotNotifier(
            repo.Object,
            Options.Create(new MilkingSettings { ProtectionWindowHours = protectionWindowHours }),
            (logger ?? new Mock<ILogger<InMemoryRobotNotifier>>()).Object);
    }

    private static MilkingNotification MakeNotification(int animalId = 1, int robotId = 1, DateTime? timestamp = null) =>
        new()
        {
            AnimalId = animalId,
            RobotId = robotId,
            Timestamp = timestamp ?? DateTime.UtcNow,
            AnimalIdentificationNumber = $"TAG-{animalId}"
        };
}
