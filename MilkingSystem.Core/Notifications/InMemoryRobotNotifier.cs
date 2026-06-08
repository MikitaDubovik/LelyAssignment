using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkingSystem.Core.Configuration;
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
public class InMemoryRobotNotifier(
    IMilkingEventRepository milkingEventRepository,
    IOptions<MilkingSettings> settings,
    ILogger<InMemoryRobotNotifier> logger) : IRobotNotifier
{
    private readonly ConcurrentDictionary<int, DateTime> _recentMilkings = new();

    private readonly List<Action<MilkingNotification>> _subscribers = [];
    private readonly Lock _subscriberLock = new();

    private readonly int _protectionWindowHours = settings.Value.ProtectionWindowHours;

    // Double-check lock for one-time hydration.
    private volatile bool _isHydrated;
    private readonly SemaphoreSlim _hydrationLock = new(1, 1);


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
                logger.LogError(ex,
                    "Milking notification subscriber {DeclaringType}.{HandlerMethod} threw an unhandled exception for AnimalId={AnimalId}, RobotId={RobotId}",
                    handler.Method.DeclaringType?.Name,
                    handler.Method.Name,
                    notification.AnimalId,
                    notification.RobotId);
            }
        }
    }

    public async Task<bool> WasRecentlyMilked(int animalId, int protectionWindowHours = 6)
    {
        await EnsureHydrated();
        return _recentMilkings.TryGetValue(animalId, out var lastMilking)
            && (DateTime.UtcNow - lastMilking).TotalHours < protectionWindowHours;
    }

    private async Task EnsureHydrated()
    {
        if (_isHydrated)
        {
            return;
        }

        await _hydrationLock.WaitAsync();
        try
        {
            if (_isHydrated)
            {
                return; // another thread hydrated while we were waiting for the lock
            }

            var recentEvents = await milkingEventRepository.GetRecentMilkingEvents(_protectionWindowHours);
            foreach (var milkingEvent in recentEvents)
            {
                _recentMilkings.AddOrUpdate(
                    milkingEvent.AnimalId,
                    milkingEvent.Timestamp,
                    (_, existing) => milkingEvent.Timestamp > existing ? milkingEvent.Timestamp : existing);
            }

            _isHydrated = true;
        }
        finally
        {
            _hydrationLock.Release();
        }
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
