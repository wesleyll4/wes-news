using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WesNews.Application.Interfaces.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuratorController(IAiCuratorService curatorService, IConfiguration configuration) : ControllerBase
{
    private const string SecretHeaderName = "X-Curator-Secret";

    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Run(CancellationToken cancellationToken = default)
    {
        string? expectedSecret = configuration["Curator:Secret"];
        string? providedSecret = Request.Headers[SecretHeaderName];

        bool hasJwt = User.Identity?.IsAuthenticated == true;
        bool hasValidSecret = !string.IsNullOrEmpty(expectedSecret) && expectedSecret == providedSecret;

        if (!hasJwt && !hasValidSecret)
            return Unauthorized(new { message = "Missing or invalid credentials." });

        await curatorService.CurateAsync(cancellationToken);
        return Accepted(new { message = "Curation completed" });
    }
}
