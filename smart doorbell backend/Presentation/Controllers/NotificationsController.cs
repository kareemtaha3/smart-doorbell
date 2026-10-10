using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;
using smart_doorbell.Application.Interfaces.Services;
using smart_doorbell.Domain.Enums;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationDispatcher _dispatcher;

    public NotificationsController(INotificationService notificationService, INotificationDispatcher dispatcher)
    {
        _notificationService = notificationService;
        _dispatcher = dispatcher;
    }

    [HttpGet("notifications/{id:guid}")]
    public async Task<ActionResult<NotificationDetailDto>> GetNotification(Guid id, CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetNotificationAsync(id, cancellationToken);
        if (notification == null)
        {
            return NotFound();
        }

        return Ok(notification);
    }

    [HttpGet("sessions/{sessionId:guid}")]
    public async Task<ActionResult<RingSessionDetailDto>> GetSessionDetails(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _notificationService.GetSessionDetailsAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return NotFound();
        }

        return Ok(session);
    }

    [HttpPost("notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        [FromQuery] Guid userId,
        [FromQuery] NotificationChannel channel)
    {
        await _dispatcher.MarkNotificationAsReadAsync(id, userId, channel);
        return Ok(new { message = "Notification marked as read." });
    }

    [HttpPost("sessions/{sessionId:guid}/ring")]
    public async Task<IActionResult> TriggerRingNotification(
        Guid sessionId,
        [FromQuery] Guid homeId,
        [FromQuery] int pressCount = 1,
        [FromQuery] string? imageUrl = null)
    {
        await _dispatcher.DispatchRingNotificationAsync(sessionId, homeId, pressCount, imageUrl);
        return Ok(new { message = "Notification dispatched." });
    }
}
