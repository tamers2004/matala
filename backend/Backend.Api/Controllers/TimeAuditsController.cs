using Backend.Api.DTOs;
using Backend.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeAuditsController : ControllerBase
{
    private readonly string _connectionString;

    public TimeAuditsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public async Task<ActionResult<List<TimeAuditResponse>>> GetByMonth(
        [FromQuery] int? year, [FromQuery] int? month)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        var audits = await TimeAudit.GetByUserAndMonthAsync(_connectionString, userId, y, m);

        var response = audits.Select(a => new TimeAuditResponse
        {
            Id = a.Id,
            Type = a.Type,
            Timestamp = a.Timestamp
        }).ToList();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<TimeAuditResponse>> Clock(ClockRequest request)
    {
        var userId = (int)HttpContext.Items["UserId"]!;

        if (request.Type is not ("clock_in" or "clock_out"))
            return BadRequest(new { message = "Type must be 'clock_in' or 'clock_out'" });

        var latest = await TimeAudit.GetLatestByUserAsync(_connectionString, userId);

        if (latest != null && latest.Type == request.Type)
            return Conflict(new { message = $"Already {request.Type.Replace('_', ' ')}ed" });

        if (latest == null && request.Type == "clock_out")
            return Conflict(new { message = "Cannot clock out without clocking in first" });

        var audit = new TimeAudit
        {
            UserId = userId,
            Type = request.Type
        };

        var id = await TimeAudit.CreateAsync(_connectionString, audit);

        var created = new TimeAuditResponse
        {
            Id = id,
            Type = audit.Type,
            Timestamp = DateTime.UtcNow
        };

        return Created($"/api/timeaudits/{id}", created);
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetStatus()
    {
        var userId = (int)HttpContext.Items["UserId"]!;

        var latest = await TimeAudit.GetLatestByUserAsync(_connectionString, userId);

        if (latest == null)
            return Ok(new { clockedIn = false });

        return Ok(new
        {
            clockedIn = latest.Type == "clock_in",
            lastType = latest.Type,
            lastTimestamp = latest.Timestamp
        });
    }
}
