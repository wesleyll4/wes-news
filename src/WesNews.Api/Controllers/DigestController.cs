using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WesNews.Application.DTOs;
using WesNews.Application.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DigestController(DigestService digestService) : ControllerBase
{
    [HttpGet("preview")]
    [ProducesResponseType(typeof(DigestPreviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview(CancellationToken cancellationToken = default)
    {
        DigestPreviewDto preview = await digestService.GetPreviewAsync(cancellationToken);
        return Ok(preview);
    }

    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Send(CancellationToken cancellationToken = default)
    {
        await digestService.SendAsync(cancellationToken);
        return Accepted();
    }
}
