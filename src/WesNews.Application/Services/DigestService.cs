using Microsoft.Extensions.Logging;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;

namespace WesNews.Application.Services;

public class DigestService(INewsArticleRepository articleRepository, IDigestEmailService emailService, IUserRepository userRepository, ILogger<DigestService> logger)
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

        IEnumerable<User> recipients = await userRepository.GetDigestEnabledUsersAsync(cancellationToken);
        List<User> recipientList = recipients.ToList();

        if (recipientList.Count == 0)
        {
            logger.LogInformation("No users with digest enabled. Skipping email sending.");
            return;
        }

        IReadOnlyList<NewsArticle> articles = await BuildDigestArticlesAsync(cancellationToken);

        foreach (User user in recipientList)
        {
            try
            {
                await emailService.SendToRecipientAsync(user.Email, articles, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send digest to {Email}", user.Email);
            }
        }
    }

    public async Task<DigestPreviewDto> GetPreviewAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsArticle> articles = await BuildDigestArticlesAsync(cancellationToken);
        string html = emailService.BuildPreviewHtml(articles);
        return new DigestPreviewDto { Html = html, ArticleCount = articles.Count };
    }
}
