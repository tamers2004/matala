using Backend.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly string _connectionString;

    public HealthController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpGet]
    public async Task<ActionResult<HealthResponse>> Get()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var cmd = new SqlCommand("SELECT 1", connection);
        var result = (int)(await cmd.ExecuteScalarAsync())!;

        return Ok(new HealthResponse { Status = "ok", Value = result });
    }
}
