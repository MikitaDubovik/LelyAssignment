using Microsoft.Data.SqlClient;
using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public class RobotRepository(string connectionString) : IRobotRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<List<Robot>> GetAllRobots()
    {
        var robots = new List<Robot>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT * FROM Robots", conn);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                robots.Add(MapRobot(reader));
            }
        }
        return robots;
    }

    public async Task<Robot?> GetRobotById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand("SELECT * FROM Robots WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapRobot(reader);
        }
        return null;
    }

    public async Task<List<Robot>> GetActiveRobots()
    {
        var robots = new List<Robot>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT * FROM Robots WHERE IsActive = 1", conn);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                robots.Add(MapRobot(reader));
            }
        }
        return robots;
    }

    private static Robot MapRobot(SqlDataReader reader)
    {
        return new Robot
        {
            Id = (int)reader["Id"],
            Name = reader["Name"].ToString()!,
            Location = reader["Location"] as string,
            IsActive = (bool)reader["IsActive"],
            CreatedAt = (DateTime)reader["CreatedAt"],
            UpdatedAt = (DateTime)reader["UpdatedAt"]
        };
    }
}
