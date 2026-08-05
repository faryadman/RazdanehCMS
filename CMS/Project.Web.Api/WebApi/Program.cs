using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    {
        option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/AddIp", GetAddIp);
app.MapGet("/api/AppSetting/{apiRoute}", AppSetting);
app.MapGet("/{apiRoute}", AppSetting);
app.MapPost("/", AppSetting);
app.MapPut("/{apiRoute}", AppSetting);
app.MapDelete("/{apiRoute}", AppSetting);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

static async Task<IResult> GetAddIp(ApplicationDbContext db, HttpContext context, HttpRequest request, int tcpCount)
{
    if (!int.TryParse(tcpCount.ToString(), out var tcpId))
    {
        return Results.BadRequest("failed");
    }
    var clientIp = context.Connection.RemoteIpAddress?.ToString();
    var userAgent = request.Headers["User-Agent"].ToString();

    var server = await db.SaveIps.SingleOrDefaultAsync(i => i.Ip == clientIp);
    if (server != null)
    {
        db.SaveIps.Remove(server);
    }
    await db.SaveIps.AddAsync(new SaveIP
    {
        Ip = clientIp,
        Tcp = tcpId.ToString(),
        UserAgent = userAgent
    });

    return Results.Ok("success");
}
static async Task<IResult> AppSetting(string apiRoute)
{
    /* var appSetting = await _appSettingService.DetailByApiRoute(apiRoute);*/
    var appSetting = "await _appSettingService.DetailByApiRoute(apiRoute);";
    return Results.Ok(apiRoute);
}

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}