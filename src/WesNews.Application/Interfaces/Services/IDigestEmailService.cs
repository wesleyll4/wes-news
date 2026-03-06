using WesNews.Domain.Entities;

namespace WesNews.Application.Interfaces.Services;

public interface IDigestEmailService
{
    Task SendAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default);
    string BuildPreviewHtml(IEnumerable<NewsArticle> articles);
}
