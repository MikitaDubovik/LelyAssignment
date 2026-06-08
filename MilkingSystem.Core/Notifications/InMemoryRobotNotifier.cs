using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Core.Notifications;

/// <summary>
/// In-memory implementation of IRobotNotifier.
/// 
/// TODO: This implementation is incomplete. Candidates should:
/// 1. Implement the Subscribe method to allow robots to receive notifications
/// 2. Implement NotifyMilkingCompleted to broadcast to all subscribers
/// 3. Implement WasRecentlyMilked to check if an animal was milked within the protection window
/// 4. Ensure thread-safety for concurrent access
/// </summary>
public class InMemoryRobotNotifier : IRobotNotifier
{
    // AnimalId - timestamp of last completed milking
    private readonly ConcurrentDictionary<int, DateTime> _recentMilkings = new();

    private readonly List<Action<MilkingNotification>> _subscribers = [];
    private readonly Lock _subscriberLock = new();
    private readonly ILogger<InMemoryRobotNotifier> _logger;

    public InMemoryRobotNotifier(IMilkingEventRepository milkingEventRepository, ILogger<InMemoryRobotNotifier> logger)
    {
        _logger = logger;

        //This is discussable. I know that it's not the common approach everywhere
        //Some projects like to have a backend that can work without a DB connection at all
        //But in this test scenario, if we can make a GET to grab necessary data then I would prefer the app to fail on Startup
        HydrateFromDatabase(milkingEventRepository);
    }

    // On startup, populate in-memory state from the DB so WasRecentlyMilked
    // is correct even if the app was recently restarted.
    private void HydrateFromDatabase(IMilkingEventRepository milkingEventRepository)
    {
        var recentEvents = milkingEventRepository.GetRecentMilkingEvents(hours: 6);
        foreach (var milkingEvent in recentEvents)
        {
            _recentMilkings.AddOrUpdate(
                milkingEvent.AnimalId,
                milkingEvent.Timestamp,
                (_, existing) => milkingEvent.Timestamp > existing ? milkingEvent.Timestamp : existing);
        }
    }

    public IDisposable Subscribe(Action<MilkingNotification> handler)
    {
        lock (_subscriberLock)
        {
            _subscribers.Add(handler);
        }

        return new Subscription(() =>
        {
            lock (_subscriberLock)
            {
                _subscribers.Remove(handler);
            }
        });
    }

    public void NotifyMilkingCompleted(MilkingNotification notification)
    {
        _recentMilkings[notification.AnimalId] = notification.Timestamp;

        List<Action<MilkingNotification>> snapshot;

        lock (_subscriberLock)
        {
            snapshot = [.. _subscribers];
        }

        foreach (var handler in snapshot)
        {
            try
            {
                handler(notification);
            }
            catch (Exception ex)
            {
                // One bad subscriber must not break the broadcast, but failures must not be silent.
                _logger.LogError(ex,
                    "Milking notification subscriber {DeclaringType}.{HandlerMethod} threw an unhandled exception for AnimalId={AnimalId}, RobotId={RobotId}",
                    handler.Method.DeclaringType?.Name,
                    handler.Method.Name,
                    notification.AnimalId,
                    notification.RobotId);
            }
        }
    }

    public bool WasRecentlyMilked(int animalId, int protectionWindowHours = 6)
    {
        return _recentMilkings.TryGetValue(animalId, out var lastMilking)
            && (DateTime.UtcNow - lastMilking).TotalHours < protectionWindowHours;
    }

    private sealed class Subscription(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose;
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) is 0)
            {
                _onDispose();
            }
        }
    }
}
