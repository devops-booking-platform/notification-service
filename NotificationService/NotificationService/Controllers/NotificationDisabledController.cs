using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Domain.DTOs;
using NotificationService.Services.Interfaces;

namespace NotificationService.Controllers;

[Route("api/notification-disabled")]
[ApiController]
[Authorize]
public class NotificationDisabledController(INotificationDisabledService notificationDisabledService) : ControllerBase
{
    [HttpPost]
    [Route("enable")]
    public async Task<IActionResult> EnableNotification([FromBody] EnableDisableNotificationRequest request)
    {
        await notificationDisabledService.EnableNotification(request);
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpPost]
    [Route("disable")]
    public async Task<IActionResult> DisableNotification([FromBody] EnableDisableNotificationRequest request)
    {
        await notificationDisabledService.DisableNotification(request);
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpGet]
    public async Task<IActionResult> GetDisabledNotifications()
    {
        var result = await notificationDisabledService.GetDisabledNotifications();
        return Ok(result);
    }
}