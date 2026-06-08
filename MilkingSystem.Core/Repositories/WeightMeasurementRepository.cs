using Microsoft.Data.SqlClient;
using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public class WeightMeasurementRepository(string connectionString) : IWeightMeasurementRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<List<WeightMeasurement>> GetWeightMeasurementsForAnimal(int animalId)
    {
        var measurements = new List<WeightMeasurement>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT * FROM WeightMeasurements WHERE AnimalId = @AnimalId", conn);
            cmd.Parameters.AddWithValue("@AnimalId", animalId);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                measurements.Add(MapWeightMeasurement(reader));
            }
        }
        return measurements;
    }

    public async Task<int> SaveWeightMeasurement(int animalId, int robotId, DateTime timestamp, decimal weightKg)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"INSERT INTO WeightMeasurements (AnimalId, RobotId, Timestamp, WeightKg)
                                       OUTPUT INSERTED.Id
                                       VALUES (@AnimalId, @RobotId, @Timestamp, @WeightKg)", conn);
        cmd.Parameters.AddWithValue("@AnimalId", animalId);
        cmd.Parameters.AddWithValue("@RobotId", robotId);
        cmd.Parameters.AddWithValue("@Timestamp", timestamp);
        cmd.Parameters.AddWithValue("@WeightKg", weightKg);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<WeightMeasurement?> GetLastWeightForAnimal(int animalId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand(
            "SELECT TOP 1 * FROM WeightMeasurements WHERE AnimalId = @AnimalId ORDER BY Timestamp DESC",
            conn);
        cmd.Parameters.AddWithValue("@AnimalId", animalId);
        var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapWeightMeasurement(reader);
        }
        return null;
    }

    private static WeightMeasurement MapWeightMeasurement(SqlDataReader reader)
    {
        return new WeightMeasurement
        {
            Id = (int)reader["Id"],
            AnimalId = (int)reader["AnimalId"],
            RobotId = (int)reader["RobotId"],
            Timestamp = (DateTime)reader["Timestamp"],
            WeightKg = (decimal)reader["WeightKg"],
            CreatedAt = (DateTime)reader["CreatedAt"]
        };
    }
}
