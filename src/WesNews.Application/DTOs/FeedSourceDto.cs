using System.ComponentModel.DataAnnotations;
using WesNews.Domain.Enums;

namespace WesNews.Application.DTOs;

public class FeedSourceDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public Category Category { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastFetchedAt { get; init; }
}

public class CreateFeedSourceRequest
{
    [Required(AllowEmptyStrings = false)]
    [MinLength(1)]
    public string Name { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MinLength(1)]
    public string Url { get; init; } = string.Empty;

    public Category Category { get; init; }
}

public class UpdateFeedSourceRequest
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
    public Category? Category { get; init; }
}
