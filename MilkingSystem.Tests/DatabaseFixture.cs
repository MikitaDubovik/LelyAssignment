namespace MilkingSystem.Tests;

/// <summary>
/// Shared fixture that provides database connection for integration tests.
/// </summary>
public class DatabaseFixture : IDisposable
{
    public string ConnectionString { get; } =
        "Server=localhost,1433;Database=MilkingSystem;User Id=sa;Password=MilkingSystem123!;TrustServerCertificate=True";

    /// <summary>
    /// Generates a unique animal identification number that is safe to use across
    /// repeated test runs without colliding on the unique constraint.
    /// </summary>
    public static string UniqueId() => $"TEST-{Guid.NewGuid().ToString("N")[..8]}";

    public void Dispose() { }
}
