namespace MilkingSystem.Tests;

/// <summary>
/// Fixture that provides database connection for integration tests.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public string ConnectionString { get; }

    public DatabaseFixture()
    {
        // Use the same connection string as the API
        ConnectionString = "Server=localhost,1433;Database=MilkingSystem;User Id=sa;Password=MilkingSystem123!;TrustServerCertificate=True";
    }

    public void Dispose()
    {
        // NOTE: No cleanup is performed here!
        // This is intentional - the flaky test relies on this.
        // Proper cleanup would involve rolling back transactions or deleting test data.
    }
}
