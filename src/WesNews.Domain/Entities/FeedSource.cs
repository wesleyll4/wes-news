using WesNews.Domain.Enums;

namespace WesNews.Domain.Entities;

public class FeedSource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Category Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastFetchedAt { get; set; }
    public ICollection<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
}
