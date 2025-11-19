using Application.Services;
using Domain.Enums;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Shared.Enums;
using static Shared.Constants;

[ApiController]
[Route("api/[controller]")]
public class TestNotificationController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IAppNotifierService _noty;
    public TestNotificationController(IHubContext<NotificationHub> hubContext, IAppNotifierService noty)
    {
        _hubContext = hubContext;
        _noty = noty;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification(string email)
    {
        //var notification = new
        //{
        //    title = "Notificación de prueba",
        //    message = "Esto es una notificación enviada desde el backend."
        //};

        //await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        //await _noty.NotifyAsync(
        //    UserRole.Admin,
        //    "Esto es una notificación enviada desde el backend.",
        //    "Notificación de prueba",
        //    level: Level.Error);

        await _noty.NotifyAsync(
            NotificationAction.Error, new { error = "Problemas." });

        return Ok("Notificación enviada");
    }

    [HttpPost("send-to-user")]
    public async Task<IActionResult> SendToUser([FromBody] TestNotificationRequest request)
    {
        var notification = new
        {
            title = "🔔 Notificación privada",
            message = $"Hola {request.UserId}, este es tu mensaje privado."
        };

        await _hubContext.Clients.User(request.UserId).SendAsync("ReceiveNotification", notification);

        return Ok("Notificación enviada a usuario " + request.UserId);
    }
}

public class TestNotificationRequest
{
    public string UserId { get; set; } = string.Empty;
}
