using Microsoft.AspNetCore.Mvc;
using smart_doorbell.Application.DTOs;
using smart_doorbell.Application.Interfaces;

namespace smart_doorbell.Presentation.Controllers;

[ApiController]
[Route("api/push-subscriptions")]
public class PushSubscriptionsController : ControllerBase
{
    private readonly IPushSubscriptionService _pushService;

    public PushSubscriptionsController(IPushSubscriptionService pushService)
    {
        _pushService = pushService;
    }

    [HttpPost]
    public async Task<ActionResult<PushSubscriptionResponseDto>> Subscribe(
        [FromQuery] Guid userId,
        [FromBody] SubscribePushDto dto)
    {
        var result = await _pushService.SubscribeAsync(userId, dto);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Unsubscribe(
        [FromQuery] Guid userId,
        [FromQuery] string endpoint)
    {
        await _pushService.UnsubscribeAsync(userId, endpoint);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PushSubscriptionResponseDto>>> GetSubscriptions([FromQuery] Guid userId)
    {
        var result = await _pushService.GetUserSubscriptionsAsync(userId);
        return Ok(result);
    }

    [HttpGet("vapid-public-key")]
    public IActionResult GetVapidPublicKey([FromServices] IConfiguration configuration)
    {
        var publicKey = configuration["Vapid:PublicKey"];
        if (string.IsNullOrWhiteSpace(publicKey))
        {
            publicKey = "BJO9uIpbqPIkbwmU-zEC93B4rV3K-xjQePEmnK5tfpI3y4eLtA_OsaWILStUzJdxnya-ZbiGnTNOpndDGuaxTPc";
        }
        return Ok(new { publicKey });
    }
}
