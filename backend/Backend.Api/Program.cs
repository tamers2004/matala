using Backend.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/timeaudits"),
    branch => branch.UseMiddleware<JwtMiddleware>()
);

app.MapControllers();

app.Run();
