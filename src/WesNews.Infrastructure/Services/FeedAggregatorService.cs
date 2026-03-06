using System.Net.Http;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using Microsoft.Extensions.Logging;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Infrastructure.Services;

public class FeedAggregatorService : IFeedAggregatorService
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FeedAggregatorService> _logger;

    private const string UserAgent =
        "Mozilla/5.0 (compatible; WesNewsBot/1.0; +https://github.com/wesleyll4/wes-news)";

    public FeedAggregatorService(
        INewsArticleRepository articleRepository,
        IHttpClientFactory httpClientFactory,
        ILogger<FeedAggregatorService> logger)
    {
        _articleRepository = articleRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task FetchAndSaveAsync(FeedSource feedSource, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] content = await FetchContentAsync(feedSource.Url, cancellationToken);

            if (!IsXmlContent(content))
            {
                _logger.LogWarning("Feed {FeedName} did not return XML — possibly blocked or redirected to HTML", feedSource.Name);
                return;
            }

            Feed feed = FeedReader.ReadFromByteArray(content);

            IEnumerable<FeedItem> feedItems = feed.Items
                .Where(item => !string.IsNullOrWhiteSpace(item.Link));

            if (feedSource.MaxItemsPerFetch.HasValue)
            {
                feedItems = feedItems.Take(feedSource.MaxItemsPerFetch.Value);
            }

            List<NewsArticle> articles = feedItems
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

    private async Task<byte[]> FetchContentAsync(string url, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("FeedAggregator");
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
        request.Headers.TryAddWithoutValidation("Accept", "application/rss+xml, application/atom+xml, application/xml, text/xml, */*");

        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private static bool IsXmlContent(byte[] content)
    {
        if (content.Length < 5)
        {
            return false;
        }

        string start = System.Text.Encoding.UTF8.GetString(content, 0, Math.Min(content.Length, 512)).TrimStart();
        return start.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
            || start.StartsWith("<rss", StringComparison.OrdinalIgnoreCase)
            || start.StartsWith("<feed", StringComparison.OrdinalIgnoreCase)
            || start.StartsWith("<atom", StringComparison.OrdinalIgnoreCase);
    }

    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty).Trim();
    }
}
