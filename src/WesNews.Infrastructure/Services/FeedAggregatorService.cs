using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Infrastructure.Services;

public class FeedAggregatorService : IFeedAggregatorService
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly ILogger<FeedAggregatorService> _logger;

    public FeedAggregatorService(INewsArticleRepository articleRepository, ILogger<FeedAggregatorService> logger)
    {
        _articleRepository = articleRepository;
        _logger = logger;
    }

    public async Task FetchAndSaveAsync(FeedSource feedSource, CancellationToken cancellationToken = default)
    {
        try
        {
            Feed feed = await FeedReader.ReadAsync(feedSource.Url);

            List<NewsArticle> articles = feed.Items
                .Where(item => !string.IsNullOrWhiteSpace(item.Link))
                .Select(item => new NewsArticle
                {
                    Id = Guid.NewGuid(),
                    Title = item.Title ?? "Untitled",
                    Summary = StripHtml(item.Description ?? string.Empty),
                    Url = item.Link!,
                    PublishedAt = item.PublishingDate ?? DateTime.UtcNow,
                    FeedSourceId = feedSource.Id,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await _articleRepository.UpsertRangeAsync(articles, cancellationToken);

            _logger.LogInformation("Fetched {Count} articles from {FeedName}", articles.Count, feedSource.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch feed {FeedName} at {Url}", feedSource.Name, feedSource.Url);
        }
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty).Trim();
    }
}
