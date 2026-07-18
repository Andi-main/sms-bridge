using SmsBridge.Relay.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "SMS Bridge Relay",
        status = "running"
    });
});

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTimeOffset.UtcNow
    });
});

app.MapHub<RelayHub>("/hubs/relay");

app.Run();