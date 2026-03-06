using WesNews.Application.DTOs;
using WesNews.Domain.Entities;

namespace WesNews.Application.Interfaces.Repositories;

public interface INewsArticleRepository
{
    Task<PagedResult<NewsArticle>> GetPagedAsync(NewsQuery query, CancellationToken cancellationToken = default);
    Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpsertRangeAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default);
}
