using System.Data.SqlClient;
using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Services;

/// <summary>
/// Main data service for the milking system.
/// Handles all database operations for animals, milking events, and weight measurements.
/// </summary>
public class DataService
{
    private string _connectionString;
    public static DataService? Instance;

    public DataService(string connectionString)
    {
        _connectionString = connectionString;
        Instance = this;
    }

    #region Animals

    public List<Animal> GetAllAnimals()
    {
        var animals = new List<Animal>();
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Animals", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animals.Add(new Animal
                {
                    Id = (int)reader["Id"],
                    IdentificationNumber = reader["IdentificationNumber"].ToString()!,
                    Name = reader["Name"] as string,
                    BirthDate = reader["BirthDate"] as DateTime?,
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                });
            }
        }
        return animals;
    }

    public Animal? GetAnimalById(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Animals WHERE Id = " + id, conn); // SQL concatenation
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Animal
                {
                    Id = (int)reader["Id"],
                    IdentificationNumber = reader["IdentificationNumber"].ToString()!,
                    Name = reader["Name"] as string,
                    BirthDate = reader["BirthDate"] as DateTime?,
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                };
            }
        }
        return null;
    }

    public Animal? GetAnimalByIdentificationNumber(string identificationNumber)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Animals WHERE IdentificationNumber = '" + identificationNumber + "'", conn);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Animal
                {
                    Id = (int)reader["Id"],
                    IdentificationNumber = reader["IdentificationNumber"].ToString()!,
                    Name = reader["Name"] as string,
                    BirthDate = reader["BirthDate"] as DateTime?,
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                };
            }
        }
        return null;
    }

    public int CreateAnimal(string identificationNumber, string? name, DateTime? birthDate)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand(@"INSERT INTO Animals (IdentificationNumber, Name, BirthDate) 
                                       OUTPUT INSERTED.Id 
                                       VALUES (@IdentificationNumber, @Name, @BirthDate)", conn);
            cmd.Parameters.AddWithValue("@IdentificationNumber", identificationNumber);
            cmd.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BirthDate", (object?)birthDate ?? DBNull.Value);
            return (int)cmd.ExecuteScalar();
        }
    }

    #endregion

    #region Robots

    public List<Robot> GetAllRobots()
    {
        var robots = new List<Robot>();
        var conn = new SqlConnection(_connectionString);
        try
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Robots", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                robots.Add(new Robot
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString()!,
                    Location = reader["Location"] as string,
                    IsActive = (bool)reader["IsActive"],
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                });
            }
        }
        catch (Exception ex)
        {
            // Swallow exception - legacy behavior
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }
        return robots;
    }

    public Robot? GetRobotById(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Robots WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
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
        return null;
    }

    public List<Robot> GetActiveRobots()
    {
        var robots = new List<Robot>();
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Robots WHERE IsActive = 1", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                robots.Add(new Robot
                {
                    Id = (int)reader["Id"],
                    Name = reader["Name"].ToString()!,
                    Location = reader["Location"] as string,
                    IsActive = (bool)reader["IsActive"],
                    CreatedAt = (DateTime)reader["CreatedAt"],
                    UpdatedAt = (DateTime)reader["UpdatedAt"]
                });
            }
        }
        return robots;
    }

    #endregion

    #region Milking Events

    public List<MilkingEvent> GetMilkingEventsForAnimal(int animalId)
    {
        var events = new List<MilkingEvent>();
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM MilkingEvents WHERE AnimalId = " + animalId + " ORDER BY Timestamp DESC", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                events.Add(MapMilkingEvent(reader));
            }
        }
        return events;
    }

    public MilkingEvent? GetLastMilkingForAnimal(int animalId)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT TOP 1 * FROM MilkingEvents WHERE AnimalId = @AnimalId ORDER BY Timestamp DESC", conn);
            cmd.Parameters.AddWithValue("@AnimalId", animalId);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapMilkingEvent(reader);
            }
        }
        return null;
    }

    public int SaveMilkingEvent(int animalId, int robotId, DateTime timestamp, decimal milkYieldLiters, int? duration)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand(@"INSERT INTO MilkingEvents (AnimalId, RobotId, Timestamp, MilkYieldLiters, Duration) 
                                       OUTPUT INSERTED.Id 
                                       VALUES (@AnimalId, @RobotId, @Timestamp, @MilkYieldLiters, @Duration)", conn);
            cmd.Parameters.AddWithValue("@AnimalId", animalId);
            cmd.Parameters.AddWithValue("@RobotId", robotId);
            cmd.Parameters.AddWithValue("@Timestamp", timestamp);
            cmd.Parameters.AddWithValue("@MilkYieldLiters", milkYieldLiters);
            cmd.Parameters.AddWithValue("@Duration", (object?)duration ?? DBNull.Value);
            return (int)cmd.ExecuteScalar();
        }
    }

    public List<MilkingEvent> GetRecentMilkingEvents(int hours = 24)
    {
        var events = new List<MilkingEvent>();
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            var cmd = new SqlCommand("SELECT * FROM MilkingEvents WHERE Timestamp > '" + cutoff.ToString("yyyy-MM-dd HH:mm:ss") + "'", conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                events.Add(MapMilkingEvent(reader));
            }
        }
        return events;
    }

    private MilkingEvent MapMilkingEvent(SqlDataReader reader)
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

    #endregion

    #region Weight Measurements

    public List<WeightMeasurement> GetWeightMeasurementsForAnimal(int animalId)
    {
        var measurements = new List<WeightMeasurement>();
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM WeightMeasurements WHERE AnimalId = " + animalId, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                measurements.Add(new WeightMeasurement
                {
                    Id = (int)reader["Id"],
                    AnimalId = (int)reader["AnimalId"],
                    RobotId = (int)reader["RobotId"],
                    Timestamp = (DateTime)reader["Timestamp"],
                    WeightKg = (decimal)reader["WeightKg"],
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }
        }
        return measurements;
    }

    public int SaveWeightMeasurement(int animalId, int robotId, DateTime timestamp, decimal weightKg)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand(@"INSERT INTO WeightMeasurements (AnimalId, RobotId, Timestamp, WeightKg) 
                                       OUTPUT INSERTED.Id 
                                       VALUES (@AnimalId, @RobotId, @Timestamp, @WeightKg)", conn);
            cmd.Parameters.AddWithValue("@AnimalId", animalId);
            cmd.Parameters.AddWithValue("@RobotId", robotId);
            cmd.Parameters.AddWithValue("@Timestamp", timestamp);
            cmd.Parameters.AddWithValue("@WeightKg", weightKg);
            return (int)cmd.ExecuteScalar();
        }
    }

    public WeightMeasurement? GetLastWeightForAnimal(int animalId)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var cmd = new SqlCommand("SELECT TOP 1 * FROM WeightMeasurements WHERE AnimalId = @AnimalId ORDER BY Timestamp DESC", conn);
            cmd.Parameters.AddWithValue("@AnimalId", animalId);
            var reader = cmd.ExecuteReader();
            if (reader.Read())
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
        return null;
    }

    #endregion

    #region Statistics - Legacy methods used by reporting

    public Dictionary<int, decimal> GetTotalMilkYieldByAnimal(DateTime from, DateTime to)
    {
        var result = new Dictionary<int, decimal>();
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        var cmd = new SqlCommand($"SELECT AnimalId, SUM(MilkYieldLiters) as Total FROM MilkingEvents WHERE Timestamp BETWEEN '{from:yyyy-MM-dd}' AND '{to:yyyy-MM-dd}' GROUP BY AnimalId", conn);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result[(int)reader["AnimalId"]] = (decimal)reader["Total"];
        }
        conn.Close();
        return result;
    }

    public double GetAverageMilkYield(int animalId)
    {
        try
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT AVG(MilkYieldLiters) FROM MilkingEvents WHERE AnimalId = " + animalId, conn);
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                    return Convert.ToDouble(result);
            }
        }
        catch
        {
            // Ignore errors
        }
        return 0;
    }

    #endregion
}
