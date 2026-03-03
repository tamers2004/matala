using Microsoft.Data.SqlClient;

namespace Backend.Api.Models;

public class TimeAudit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public static async Task<List<TimeAudit>> GetByUserAndMonthAsync(
        string connectionString, int userId, int year, int month)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(@"
            SELECT id, user_id, type, timestamp
            FROM time_audits
            WHERE user_id = @UserId
              AND YEAR(timestamp) = @Year
              AND MONTH(timestamp) = @Month
            ORDER BY timestamp DESC", connection);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Year", year);
        cmd.Parameters.AddWithValue("@Month", month);

        var results = new List<TimeAudit>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new TimeAudit
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Type = reader.GetString(2),
                Timestamp = reader.GetDateTime(3)
            });
        }

        return results;
    }

    public static async Task<int> CreateAsync(string connectionString, TimeAudit audit)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(@"
            INSERT INTO time_audits (user_id, type)
            OUTPUT INSERTED.id
            VALUES (@UserId, @Type)", connection);
        cmd.Parameters.AddWithValue("@UserId", audit.UserId);
        cmd.Parameters.AddWithValue("@Type", audit.Type);

        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public static async Task<TimeAudit?> GetLatestByUserAsync(string connectionString, int userId)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand(@"
            SELECT TOP 1 id, user_id, type, timestamp
            FROM time_audits
            WHERE user_id = @UserId
            ORDER BY timestamp DESC", connection);
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new TimeAudit
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            Type = reader.GetString(2),
            Timestamp = reader.GetDateTime(3)
        };
    }
}
