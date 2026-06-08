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
    // TODO: Add necessary fields for tracking subscriptions and recent milkings

    public void NotifyMilkingCompleted(MilkingNotification notification)
    {
        // TODO: Implement broadcasting to all subscribers
        // Consider: What happens if a subscriber throws an exception?
        // Consider: Should this be synchronous or asynchronous?
        throw new NotImplementedException("Candidate should implement this method");
    }

    public IDisposable Subscribe(Action<MilkingNotification> handler)
    {
        // TODO: Implement subscription mechanism
        // Return an IDisposable that removes the subscription when disposed
        throw new NotImplementedException("Candidate should implement this method");
    }

    public bool WasRecentlyMilked(int animalId, int protectionWindowHours = 6)
    {
        // TODO: Implement check for recent milking within the protection window
        // This should be thread-safe and efficient
        throw new NotImplementedException("Candidate should implement this method");
    }
}
