using System.ComponentModel.DataAnnotations;

namespace Backend.Api.DTOs;

public class ClockRequest
{
    [Required]
    public string Type { get; set; } = string.Empty;
}
