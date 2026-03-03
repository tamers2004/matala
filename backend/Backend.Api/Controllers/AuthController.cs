using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using UserModel = Backend.Api.Models.User;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthResponse>> Signup(SignupRequest request)
    {
        if (await UserModel.EmailExistsAsync(_connectionString, request.Email))
            return Conflict(new { message = "Email already registered" });

        var user = new UserModel
        {
            Email = request.Email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var userId = await UserModel.CreateAsync(_connectionString, user);
        var token = GenerateToken(userId, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await UserModel.GetByEmailAsync(_connectionString, request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = GenerateToken(user.Id, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        });
    }

    private string GenerateToken(int userId, string email)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };

        var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]!);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
