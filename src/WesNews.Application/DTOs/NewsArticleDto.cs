using WesNews.Domain.Enums;

namespace WesNews.Application.DTOs;

public class NewsArticleDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public DateTime PublishedAt { get; init; }
    public bool IsRead { get; init; }
    public bool IsFeatured { get; init; }
    public DateTime? FeaturedAt { get; init; }
    public Category Category { get; init; }
    public string FeedSourceName { get; init; } = string.Empty;
}
