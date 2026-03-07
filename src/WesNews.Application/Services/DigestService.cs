using Microsoft.Extensions.Logging;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;

namespace WesNews.Application.Services;

public class DigestService(INewsArticleRepository articleRepository, IDigestEmailService emailService, ILogger<DigestService> logger)
{
    private const int ArticlesPerCategory = 5;

    public async Task<IReadOnlyList<NewsArticle>> BuildDigestArticlesAsync(CancellationToken cancellationToken = default)
    {
        List<NewsArticle> allArticles = new List<NewsArticle>();

        foreach (Category category in Enum.GetValues<Category>())
        {
            NewsQuery query = new()
            {
                Category = category,
                UnreadOnly = true,
                Page = 1,
                PageSize = ArticlesPerCategory
            };

            PagedResult<NewsArticle> result = await articleRepository.GetPagedAsync(query, cancellationToken);
            allArticles.AddRange(result.Items);
        }

        return allArticles.AsReadOnly();
    }

    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting digest email generation and sending process");
        IReadOnlyList<NewsArticle> articles = await BuildDigestArticlesAsync(cancellationToken);
        await emailService.SendAsync(articles, cancellationToken);
    }

    public async Task<DigestPreviewDto> GetPreviewAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsArticle> articles = await BuildDigestArticlesAsync(cancellationToken);
        string html = emailService.BuildPreviewHtml(articles);
        return new DigestPreviewDto { Html = html, ArticleCount = articles.Count };
    }
}
