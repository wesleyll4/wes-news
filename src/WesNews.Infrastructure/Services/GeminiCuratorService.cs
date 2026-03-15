using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;
using WesNews.Infrastructure.Configuration;

namespace WesNews.Infrastructure.Services;

public class GeminiCuratorService : IAiCuratorService
{
    private readonly INewsArticleRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiCuratorService> _logger;

    private static readonly Category[] Categories = Enum.GetValues<Category>();

    public GeminiCuratorService(
        INewsArticleRepository repository,
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiOptions> options,
        ILogger<GeminiCuratorService> logger)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task CurateAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Gemini API key not configured — skipping curation");
            return;
        }

        _logger.LogInformation("Starting AI curation for {Count} categories", Categories.Length);

        foreach (Category category in Categories)
        {
            await CurateCategoryAsync(category, cancellationToken);
        }

        _logger.LogInformation("AI curation completed");
    }

    private async Task CurateCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<NewsArticle> candidates = await _repository.GetRecentByCategoryAsync(
                category,
                _options.CandidateLookbackHours,
                _options.CandidateLimit,
                cancellationToken);

            if (candidates.Count < _options.TopPicksPerCategory)
            {
                _logger.LogDebug("Not enough candidates for {Category} ({Count}) — skipping", category, candidates.Count);
                return;
            }

            List<Guid> featuredIds = await RankWithGeminiAsync(category, candidates, cancellationToken);

            if (featuredIds.Count == 0)
            {
                _logger.LogWarning("Gemini returned no valid IDs for {Category}", category);
                return;
            }

            await _repository.ClearFeaturedByCategoryAsync(category, cancellationToken);
            await _repository.SetFeaturedAsync(featuredIds, cancellationToken);

            _logger.LogInformation("Curated {Count} featured articles for {Category}", featuredIds.Count, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to curate category {Category}", category);
        }
    }

    private async Task<List<Guid>> RankWithGeminiAsync(
        Category category,
        IReadOnlyList<NewsArticle> candidates,
        CancellationToken cancellationToken)
    {
        string articleList = string.Join("\n", candidates.Select((a, i) =>
            $"{i + 1}. ID:{a.Id} | {a.Title} | {a.Summary?[..Math.Min(a.Summary?.Length ?? 0, 120)]}"));

        string prompt = $"""
            You are a senior software engineer and tech lead curating a personal news feed.
            Category: {category}

            Below are the {candidates.Count} most recent articles from this category (last {_options.CandidateLookbackHours}h).
            Select the {_options.TopPicksPerCategory} most relevant/impactful articles for a developer audience.
            Criteria: technical impact (breaking changes, new frameworks, critical vulnerabilities, architecture shifts) + industry buzz/virality.

            Articles:
            {articleList}

            Respond ONLY with a JSON array containing exactly {_options.TopPicksPerCategory} article IDs (GUIDs), ordered by relevance descending.
            Example: ["guid1","guid2","guid3"]
            Do not include any explanation or markdown.
            """;

        string requestBody = JsonSerializer.Serialize(new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.2
            }
        });

        HttpClient client = _httpClientFactory.CreateClient("Gemini");
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
        };

        using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error {Status} for {Category}: {Body}", response.StatusCode, category, responseBody[..Math.Min(200, responseBody.Length)]);
            return [];
        }

        return ParseGeminiResponse(responseBody, candidates);
    }

    private List<Guid> ParseGeminiResponse(string responseBody, IReadOnlyList<NewsArticle> candidates)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            string rawText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            string cleaned = rawText.Trim().TrimStart('`');
            if (cleaned.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[4..];
            }
            cleaned = cleaned.TrimEnd('`').Trim();

            List<string> rawIds = JsonSerializer.Deserialize<List<string>>(cleaned) ?? [];

            HashSet<Guid> validIds = candidates.Select(a => a.Id).ToHashSet();

            List<Guid> result = rawIds
                .Where(id => Guid.TryParse(id, out Guid g) && validIds.Contains(g))
                .Select(id => Guid.Parse(id))
                .Take(_options.TopPicksPerCategory)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response");
            return [];
        }
    }
}
