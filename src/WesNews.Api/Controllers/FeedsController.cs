using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WesNews.Application.DTOs;
using WesNews.Application.Services;

namespace WesNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedsController(FeedService feedService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<FeedSourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeeds(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FeedSourceDto> feeds = await feedService.GetAllAsync(cancellationToken);
        return Ok(feeds);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(FeedSourceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddFeed([FromBody] CreateFeedSourceRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            FeedSourceDto created = await feedService.AddAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetFeeds), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeed(Guid id, [FromBody] UpdateFeedSourceRequest request, CancellationToken cancellationToken = default)
    {
        bool success = await feedService.UpdateAsync(id, request, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeed(Guid id, CancellationToken cancellationToken = default)
    {
        bool success = await feedService.DeleteAsync(id, cancellationToken);
        return success ? NoContent() : NotFound();
    }
}
