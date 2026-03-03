using Microsoft.Data.SqlClient;

namespace Backend.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static async Task<bool> EmailExistsAsync(string connectionString, string email)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM users WHERE email = @Email", connection);
        cmd.Parameters.AddWithValue("@Email", email);

        return (int)(await cmd.ExecuteScalarAsync())! > 0;
    }

    public static async Task<int> CreateAsync(string connectionString, User user)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(@"
            INSERT INTO users (email, hashed_password, first_name, last_name)
            OUTPUT INSERTED.id
            VALUES (@Email, @HashedPassword, @FirstName, @LastName)", connection);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@HashedPassword", user.HashedPassword);
        cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
        cmd.Parameters.AddWithValue("@LastName", user.LastName);

        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public static async Task<User?> GetByEmailAsync(string connectionString, string email)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(
            "SELECT id, email, hashed_password, first_name, last_name, enabled, created_at, updated_at FROM users WHERE email = @Email AND enabled = 1",
            connection);
        cmd.Parameters.AddWithValue("@Email", email);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetInt32(0),
            Email = reader.GetString(1),
            HashedPassword = reader.GetString(2),
            FirstName = reader.GetString(3),
            LastName = reader.GetString(4),
            Enabled = reader.GetBoolean(5),
            CreatedAt = reader.GetDateTime(6),
            UpdatedAt = reader.GetDateTime(7)
        };
    }
}
