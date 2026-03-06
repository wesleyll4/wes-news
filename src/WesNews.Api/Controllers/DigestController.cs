using Microsoft.AspNetCore.Mvc;
using WesNews.Application.DTOs;
using WesNews.Application.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DigestController : ControllerBase
{
    private readonly DigestService _digestService;

    public DigestController(DigestService digestService)
    {
        _digestService = digestService;
    }

    [HttpGet("preview")]
    [ProducesResponseType(typeof(DigestPreviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview(CancellationToken cancellationToken = default)
    {
        DigestPreviewDto preview = await _digestService.GetPreviewAsync(cancellationToken);
        return Ok(preview);
    }

    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Send(CancellationToken cancellationToken = default)
    {
        await _digestService.SendAsync(cancellationToken);
        return Accepted();
    }
}
