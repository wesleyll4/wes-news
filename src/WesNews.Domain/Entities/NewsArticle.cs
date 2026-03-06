namespace WesNews.Domain.Entities;

public class NewsArticle
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid FeedSourceId { get; set; }
    public FeedSource FeedSource { get; set; } = null!;
}
