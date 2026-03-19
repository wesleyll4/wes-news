using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WesNews.Application.DTOs;
using WesNews.Application.Services;
using WesNews.Domain.Enums;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(NewsService newsService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<NewsArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNews(
        [FromQuery] Category? category,
        [FromQuery] string? search,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;

        NewsQuery query = new NewsQuery
        {
            Category = category,
            Search = search,
            UnreadOnly = isAuthenticated && unreadOnly,
            Page = page,
            PageSize = pageSize
        };

        PagedResult<NewsArticleDto> result = await newsService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken = default)
    {
        bool success = await newsService.MarkAsReadAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        bool success = await newsService.DeleteAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
