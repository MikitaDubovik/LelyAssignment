namespace MilkingSystem.Core.Notifications;

/// <summary>
/// Interface for notifying robots about milking events.
/// 
/// When a robot completes milking an animal, it notifies our system. We then broadcast
/// this information to all other robots so they know not to milk that cow again.
/// 
/// Scenario: A cow gets milked by Robot A. She then walks over to Robot B hoping for 
/// more food. Robot B checks with our system and learns she was recently milked, so 
/// Robot B sends her away without milking.
/// 
/// In production, this would communicate over a low-level TCP socket protocol
/// to the robot controllers. For this implementation, an in-memory solution is acceptable.
/// </summary>
public interface IRobotNotifier
{
    /// <summary>
    /// Notify all robots that a milking event has been completed.
    /// Other robots will use this information to avoid milking the same cow.
    /// </summary>
    /// <param name="notification">The milking notification details.</param>
    void NotifyMilkingCompleted(MilkingNotification notification);

    /// <summary>
    /// Subscribe to milking notifications.
    /// </summary>
    /// <param name="handler">The handler to call when a notification is received.</param>
    /// <returns>A subscription that can be disposed to unsubscribe.</returns>
    IDisposable Subscribe(Action<MilkingNotification> handler);

    /// <summary>
    /// Check if an animal was recently milked (within the protection window).
    /// Robots call this before starting a milking to check if the cow should be sent away.
    /// </summary>
    /// <param name="animalId">The animal ID to check.</param>
    /// <param name="protectionWindowHours">The number of hours to check back (default 6 hours).</param>
    /// <returns>True if the animal was recently milked and should NOT be milked again.</returns>
    bool WasRecentlyMilked(int animalId, int protectionWindowHours = 6);
}
