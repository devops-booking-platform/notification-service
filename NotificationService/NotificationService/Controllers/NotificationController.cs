using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Domain.DTOs;
using NotificationService.Services.Interfaces;

namespace NotificationService.Controllers;

[Route("api/notification")]
[ApiController]
[Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkNotificationAsReadCommand command)
    {
        await notificationService.MarkAsRead(command.Id);
        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] GetNotificationRequest request)
    {
        var result = await notificationService.GetNotifications(request);
        return Ok(result);
    }

    [HttpGet]
    [Route("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await notificationService.GetUnreadNotifications();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await notificationService.GetNotification(id);
        return Ok(result);
    }
}