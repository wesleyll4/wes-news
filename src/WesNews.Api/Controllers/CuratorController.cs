using Microsoft.AspNetCore.Mvc;
using WesNews.Application.Interfaces.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuratorController(
    IAiCuratorService curatorService,
    IConfiguration configuration,
    ILogger<CuratorController> logger) : ControllerBase
{
    private const string SecretHeaderName = "X-Curator-Secret";

    [HttpPost("run")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Run()
    {
        string? expectedSecret = configuration["Curator:Secret"];
        string? providedSecret = Request.Headers[SecretHeaderName];

        bool hasJwt = User.Identity?.IsAuthenticated == true;
        bool hasValidSecret = !string.IsNullOrEmpty(expectedSecret) && expectedSecret == providedSecret;

        if (!hasJwt && !hasValidSecret)
            return Unauthorized(new { message = "Missing or invalid credentials." });

        _ = Task.Run(async () =>
        {
            try
            {
                await curatorService.CurateAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background curation failed.");
            }
        });

        return Accepted(new { message = "Curation started in background." });
    }
}
