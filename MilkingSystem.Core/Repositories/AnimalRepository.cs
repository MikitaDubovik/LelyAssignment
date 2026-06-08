using Microsoft.Data.SqlClient;
using MilkingSystem.Core.Models;

namespace MilkingSystem.Core.Repositories;

public class AnimalRepository(string connectionString) : IAnimalRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<List<Animal>> GetAllAnimals()
    {
        var animals = new List<Animal>();
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT * FROM Animals", conn);
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                animals.Add(MapAnimal(reader));
            }
        }
        return animals;
    }

    public async Task<Animal?> GetAnimalById(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand("SELECT * FROM Animals WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAnimal(reader);
        }
        return null;
    }

    public async Task<Animal?> GetAnimalByIdentificationNumber(string identificationNumber)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand("SELECT * FROM Animals WHERE IdentificationNumber = @IdentificationNumber", conn);
        cmd.Parameters.AddWithValue("@IdentificationNumber", identificationNumber);
        var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAnimal(reader);
        }
        return null;
    }

    public async Task<int> CreateAnimal(string identificationNumber, string? name, DateTime? birthDate)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"INSERT INTO Animals (IdentificationNumber, Name, BirthDate)
                                       OUTPUT INSERTED.Id
                                       VALUES (@IdentificationNumber, @Name, @BirthDate)", conn);
        cmd.Parameters.AddWithValue("@IdentificationNumber", identificationNumber);
        cmd.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BirthDate", (object?)birthDate ?? DBNull.Value);
        return (int)(await cmd.ExecuteScalarAsync())!;
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
