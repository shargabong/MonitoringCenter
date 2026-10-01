using LiveMonitoringCenter;
using LiveMonitoringCenter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ConnectionsTracker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

app.MapHub<MonitoringHub>("/monitoringHub");

app.MapPost("/api/system-message", async (SystemMessageRequest request, ConnectionsTracker tracker) =>
{
    await tracker.SendSystemMessageAsync(request.Message);
    return Results.Ok(new { status = "Success" });
});

app.MapPost("/api/room-notification", async (RoomNotificationRequest request, ConnectionsTracker tracker) =>
{
    await tracker.SendExternalRoomNotificationAsync(request.RoomName, request.Message);
    return Results.Ok(new { status = "Success" });
});

app.MapPost("/api/private-notification", async (PrivateNotificationRequest request, ConnectionsTracker tracker) =>
{
    await tracker.SendExternalPrivateMessageAsync(request.ConnectionId, request.Message);
    return Results.Ok(new { status = "Success" });
});

app.Run();

public record SystemMessageRequest(string Message);
public record RoomNotificationRequest(string RoomName, string Message);
public record PrivateNotificationRequest(string ConnectionId, string Message);
