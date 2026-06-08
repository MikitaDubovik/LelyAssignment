using Microsoft.Data.SqlClient;
using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public class AnimalRepository(string connectionString) : IAnimalRepository
{
    private readonly string _connectionString = connectionString;

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
                animals.Add(MapAnimal(reader));
            }
        }
        return animals;
    }

    public Animal? GetAnimalById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var cmd = new SqlCommand("SELECT * FROM Animals WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapAnimal(reader);
        }
        return null;
    }

    public Animal? GetAnimalByIdentificationNumber(string identificationNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var cmd = new SqlCommand("SELECT * FROM Animals WHERE IdentificationNumber = @IdentificationNumber", conn);
        cmd.Parameters.AddWithValue("@IdentificationNumber", identificationNumber);
        var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return MapAnimal(reader);
        }
        return null;
    }

    public int CreateAnimal(string identificationNumber, string? name, DateTime? birthDate)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var cmd = new SqlCommand(@"INSERT INTO Animals (IdentificationNumber, Name, BirthDate)
                                       OUTPUT INSERTED.Id
                                       VALUES (@IdentificationNumber, @Name, @BirthDate)", conn);
        cmd.Parameters.AddWithValue("@IdentificationNumber", identificationNumber);
        cmd.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BirthDate", (object?)birthDate ?? DBNull.Value);
        return (int)cmd.ExecuteScalar()!;
    }

    private static Animal MapAnimal(SqlDataReader reader)
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
