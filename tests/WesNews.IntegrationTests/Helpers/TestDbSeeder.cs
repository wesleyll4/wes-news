using WesNews.Domain.Entities;
using WesNews.Domain.Enums;
using WesNews.Infrastructure.Data;

namespace WesNews.IntegrationTests.Helpers;

public static class TestDbSeeder
{
    public static readonly Guid FeedId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Article1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Article2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task SeedAsync(AppDbContext context)
    {
        FeedSource feed = new FeedSource
        {
            Id = FeedId,
            Name = "Test Feed",
            Url = "https://test-feed.com/rss",
            Category = Category.DotNet,
            IsActive = true
        };

        NewsArticle article1 = new NewsArticle
        {
            Id = Article1Id,
            Title = "Test Article 1",
            Summary = "Summary of the first test article",
            Url = "https://test.com/article-1",
            PublishedAt = DateTime.UtcNow.AddHours(-2),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            FeedSourceId = feed.Id,
            FeedSource = feed
        };

        NewsArticle article2 = new NewsArticle
        {
            Id = Article2Id,
            Title = "Blazor Deep Dive",
            Summary = "A deep dive into Blazor components",
            Url = "https://test.com/article-2",
            PublishedAt = DateTime.UtcNow.AddHours(-1),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            FeedSourceId = feed.Id,
            FeedSource = feed
        };

        context.FeedSources.Add(feed);
        context.NewsArticles.AddRange(article1, article2);
        await context.SaveChangesAsync();
    }
}
