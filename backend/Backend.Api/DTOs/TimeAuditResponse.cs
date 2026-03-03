namespace Backend.Api.DTOs;

public class TimeAuditResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
