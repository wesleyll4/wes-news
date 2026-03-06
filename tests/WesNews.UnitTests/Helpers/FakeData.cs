using WesNews.Domain.Entities;
using WesNews.Domain.Enums;

namespace WesNews.UnitTests.Helpers;

public static class FakeData
{
    public static FeedSource CreateFeedSource(
        Guid? id = null,
        string name = "Test Feed",
        string url = "https://example.com/feed",
        Category category = Category.DotNet,
        bool isActive = true)
    {
        return new FeedSource
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Url = url,
            Category = category,
            IsActive = isActive
        };
    }

    public static NewsArticle CreateNewsArticle(
        Guid? id = null,
        string title = "Test Article",
        string url = "https://example.com/article",
        bool isRead = false,
        FeedSource? feedSource = null)
    {
        FeedSource source = feedSource ?? CreateFeedSource();

        return new NewsArticle
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Summary = "Test summary content",
            Url = url,
            PublishedAt = DateTime.UtcNow.AddHours(-1),
            IsRead = isRead,
            CreatedAt = DateTime.UtcNow,
            FeedSourceId = source.Id,
            FeedSource = source
        };
    }

    public static IReadOnlyList<NewsArticle> CreateNewsArticles(int count, bool isRead = false)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateNewsArticle(
                title: $"Article {i}",
                url: $"https://example.com/article-{i}-{Guid.NewGuid()}",
                isRead: isRead))
            .ToList()
            .AsReadOnly();
    }

    public static IReadOnlyList<FeedSource> CreateFeedSources(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateFeedSource(
                name: $"Feed {i}",
                url: $"https://example.com/feed-{i}"))
            .ToList()
            .AsReadOnly();
    }
}
