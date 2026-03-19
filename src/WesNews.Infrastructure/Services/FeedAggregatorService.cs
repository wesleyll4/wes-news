using System.Net.Http;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using Microsoft.Extensions.Logging;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Infrastructure.Services;

public class FeedAggregatorService(
    INewsArticleRepository articleRepository,
    IHttpClientFactory httpClientFactory,
    ILogger<FeedAggregatorService> logger) : IFeedAggregatorService
{
    private const string UserAgent =
        "Mozilla/5.0 (compatible; WesNewsBot/1.0; +https://github.com/wesleyll4/wes-news)";

    public async Task FetchAndSaveAsync(FeedSource feedSource, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] content = await FetchContentAsync(feedSource.Url, cancellationToken);

            if (!IsXmlContent(content))
            {
                logger.LogWarning("Feed {FeedName} did not return XML — possibly blocked or redirected to HTML", feedSource.Name);
                return;
            }

            Feed feed = FeedReader.ReadFromByteArray(content);

            DateTime cutoff = DateTime.UtcNow.AddDays(-7);

            IEnumerable<FeedItem> feedItems = feed.Items
                .Where(item => !string.IsNullOrWhiteSpace(item.Link))
                .Where(item => (item.PublishingDate ?? DateTime.UtcNow) >= cutoff);

            if (feedSource.MaxItemsPerFetch.HasValue)
            {
                feedItems = feedItems.Take(feedSource.MaxItemsPerFetch.Value);
            }

            List<NewsArticle> articles = [.. feedItems
                .Select(item => new NewsArticle
                {
                    Id = Guid.NewGuid(),
                    Title = item.Title ?? "Untitled",
                    Summary = StripHtml(item.Description ?? string.Empty),
                    Url = item.Link!,
                    ImageUrl = ExtractImageUrl(item),
                    PublishedAt = item.PublishingDate ?? DateTime.UtcNow,
                    FeedSourceId = feedSource.Id,
                    CreatedAt = DateTime.UtcNow
                })];

            await articleRepository.UpsertRangeAsync(articles, cancellationToken);

            logger.LogDebug("Fetched {Count} articles from {FeedName}", articles.Count, feedSource.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch feed {FeedName} at {Url}", feedSource.Name, feedSource.Url);
        }
    }

    private async Task<byte[]> FetchContentAsync(string url, CancellationToken cancellationToken)
    {
        HttpClient client = httpClientFactory.CreateClient("FeedAggregator");
        using HttpRequestMessage request = new(HttpMethod.Get, url);
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

    private static string? ExtractImageUrl(FeedItem item)
    {
        if (item.SpecificItem is Rss20FeedItem rss20Item
            && rss20Item.Enclosure != null
            && rss20Item.Enclosure.MediaType != null
            && rss20Item.Enclosure.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return rss20Item.Enclosure.Url;
        }

        System.Xml.Linq.XElement? mediaContent = item.SpecificItem?.Element?.Element(System.Xml.Linq.XName.Get("content", "http://search.yahoo.com/mrss/"));
        if (mediaContent != null)
        {
            return mediaContent.Attribute("url")?.Value;
        }

        return null;
    }
}
