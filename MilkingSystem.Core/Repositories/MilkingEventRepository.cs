using Microsoft.Data.SqlClient;
using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public class MilkingEventRepository(string connectionString) : IMilkingEventRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<List<MilkingEvent>> GetMilkingEventsForAnimal(int animalId)
    {
        var events = new List<MilkingEvent>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT * FROM MilkingEvents WHERE AnimalId = @AnimalId ORDER BY Timestamp DESC", conn);
            cmd.Parameters.AddWithValue("@AnimalId", animalId);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                events.Add(MapMilkingEvent(reader));
            }
        }
        return events;
    }

    public async Task<MilkingEvent?> GetLastMilkingForAnimal(int animalId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand("SELECT TOP 1 * FROM MilkingEvents WHERE AnimalId = @AnimalId ORDER BY Timestamp DESC", conn);
        cmd.Parameters.AddWithValue("@AnimalId", animalId);
        var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapMilkingEvent(reader);
        }
        return null;
    }

    public async Task<int> SaveMilkingEvent(int animalId, int robotId, DateTime timestamp, decimal milkYieldLiters, int? duration)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"INSERT INTO MilkingEvents (AnimalId, RobotId, Timestamp, MilkYieldLiters, Duration)
                                       OUTPUT INSERTED.Id
                                       VALUES (@AnimalId, @RobotId, @Timestamp, @MilkYieldLiters, @Duration)", conn);
        cmd.Parameters.AddWithValue("@AnimalId", animalId);
        cmd.Parameters.AddWithValue("@RobotId", robotId);
        cmd.Parameters.AddWithValue("@Timestamp", timestamp);
        cmd.Parameters.AddWithValue("@MilkYieldLiters", milkYieldLiters);
        cmd.Parameters.AddWithValue("@Duration", (object?)duration ?? DBNull.Value);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<List<MilkingEvent>> GetRecentMilkingEvents(int hours = 24)
    {
        var events = new List<MilkingEvent>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            var cmd = new SqlCommand("SELECT * FROM MilkingEvents WHERE Timestamp > @Cutoff", conn);
            cmd.Parameters.AddWithValue("@Cutoff", cutoff);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                events.Add(MapMilkingEvent(reader));
            }
        }
        return events;
    }

    public async Task<Dictionary<int, decimal>> GetTotalMilkYieldByAnimal(DateTime from, DateTime to)
    {
        var result = new Dictionary<int, decimal>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new SqlCommand(
                "SELECT AnimalId, SUM(MilkYieldLiters) as Total FROM MilkingEvents WHERE Timestamp BETWEEN @From AND @To GROUP BY AnimalId",
                conn);
            cmd.Parameters.AddWithValue("@From", from);
            cmd.Parameters.AddWithValue("@To", to);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result[(int)reader["AnimalId"]] = (decimal)reader["Total"];
            }
        }
        return result;
    }

    public async Task<double> GetAverageMilkYield(int animalId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand("SELECT AVG(MilkYieldLiters) FROM MilkingEvents WHERE AnimalId = @AnimalId", conn);
        cmd.Parameters.AddWithValue("@AnimalId", animalId);
        var result = await cmd.ExecuteScalarAsync();
        if (result != DBNull.Value)
        {
            return Convert.ToDouble(result);
        }
        return 0;
    }

    private static MilkingEvent MapMilkingEvent(SqlDataReader reader)
    {
        return new MilkingEvent
        {
            Id = (int)reader["Id"],
            AnimalId = (int)reader["AnimalId"],
            RobotId = (int)reader["RobotId"],
            Timestamp = (DateTime)reader["Timestamp"],
            MilkYieldLiters = (decimal)reader["MilkYieldLiters"],
            Duration = reader["Duration"] as int?,
            CreatedAt = (DateTime)reader["CreatedAt"]
        };
    }
}
