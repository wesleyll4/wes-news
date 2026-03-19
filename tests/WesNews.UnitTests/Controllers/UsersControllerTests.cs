using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WesNews.Api.Controllers;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Services;

namespace WesNews.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly IUserService _userService;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _userService = Substitute.For<IUserService>();
        _sut = new UsersController(_userService);
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        // No ClaimTypes.NameIdentifier claim — simulates missing/invalid identity
        var identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }

    // Sub-task 2.3: Unit tests for DeleteMe endpoint

    [Fact]
    public async Task DeleteMe_WithNoNameIdentifierClaim_Returns401()
    {
        // Arrange — missing NameIdentifier claim (Req 1.3)
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateUnauthenticatedUser() }
        };

        // Act
        IActionResult result = await _sut.DeleteMe();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task DeleteMe_WhenUserNotFound_Returns404()
    {
        // Arrange — valid auth but service throws KeyNotFoundException (Req 1.4)
        var userId = Guid.NewGuid();
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateAuthenticatedUser(userId) }
        };

        _userService
            .DeleteAccountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(_ => throw new KeyNotFoundException());

        // Act
        IActionResult result = await _sut.DeleteMe();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateDigestPreference_WithNoNameIdentifierClaim_Returns401()
    {
        // Arrange — controller has no valid NameIdentifier claim (Req 2.3)
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateUnauthenticatedUser() }
        };

        var request = new UpdateDigestPreferenceRequest { DigestEnabled = true };

        // Act
        IActionResult result = await _sut.UpdateDigestPreference(request);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task UpdateDigestPreference_WhenUserNotFound_Returns404()
    {
        // Arrange — valid auth but service throws KeyNotFoundException
        var userId = Guid.NewGuid();
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = CreateAuthenticatedUser(userId) }
        };

        _userService
            .UpdateDigestPreferenceAsync(userId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns<DigestPreferenceResponse>(_ => throw new KeyNotFoundException());

        var request = new UpdateDigestPreferenceRequest { DigestEnabled = true };

        // Act
        IActionResult result = await _sut.UpdateDigestPreference(request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
