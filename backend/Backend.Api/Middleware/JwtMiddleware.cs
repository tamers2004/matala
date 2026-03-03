using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _key;
    private readonly string _issuer;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _key = configuration["Jwt:Key"]!;
        _issuer = configuration["Jwt:Issuer"]!;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Missing or invalid token" });
            return;
        }

        var token = authHeader["Bearer ".Length..];

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.InboundClaimTypeMap.Clear();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
            };

            var principal = tokenHandler.ValidateToken(token, validationParams, out _);
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirst("sub");

            if (userIdClaim == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid token claims" });
                return;
            }

            context.Items["UserId"] = int.Parse(userIdClaim.Value);
        }
        catch
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired token" });
            return;
        }

        await _next(context);
    }
}
