using Microsoft.AspNetCore.Mvc;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        LoginResponse? result = await authService.LoginAsync(request, cancellationToken);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        bool success = await authService.RegisterAsync(request, cancellationToken);

        if (!success)
        {
            return BadRequest(new { message = "Email already in use" });
        }

        return Ok(new { message = "User registered successfully" });
    }
}