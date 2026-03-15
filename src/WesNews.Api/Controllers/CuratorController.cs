using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WesNews.Application.Interfaces.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CuratorController(IAiCuratorService curatorService) : ControllerBase
{
    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Run(CancellationToken cancellationToken = default)
    {
        await curatorService.CurateAsync(cancellationToken);
        return Accepted(new { message = "Curation completed" });
    }
}
