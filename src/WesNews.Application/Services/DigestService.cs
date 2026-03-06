using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;

namespace WesNews.Application.Services;

public class DigestService
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly IDigestEmailService _emailService;
    private const int ArticlesPerCategory = 5;

    public DigestService(INewsArticleRepository articleRepository, IDigestEmailService emailService)
    {
        _articleRepository = articleRepository;
        _emailService = emailService;
    }

    public async Task<IReadOnlyList<NewsArticle>> BuildDigestArticlesAsync(CancellationToken cancellationToken = default)
    {
        List<NewsArticle> allArticles = new List<NewsArticle>();

        foreach (Category category in Enum.GetValues<Category>())
        {
            NewsQuery query = new NewsQuery
            {
                Category = category,
                UnreadOnly = true,
                Page = 1,
                PageSize = ArticlesPerCategory
            };

            PagedResult<NewsArticle> result = await _articleRepository.GetPagedAsync(query, cancellationToken);
            allArticles.AddRange(result.Items);
        }

        return allArticles.AsReadOnly();
    }

    public async Task SendAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsArticle> articles = await BuildDigestArticlesAsync(cancellationToken);
        await _emailService.SendAsync(articles, cancellationToken);
    }

    public async Task<DigestPreviewDto> GetPreviewAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsArticle> articles = await BuildDigestArticlesAsync(cancellationToken);
        string html = _emailService.BuildPreviewHtml(articles);
        return new DigestPreviewDto { Html = html, ArticleCount = articles.Count };
    }
}
