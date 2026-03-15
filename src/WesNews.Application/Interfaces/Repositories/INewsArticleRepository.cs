using WesNews.Application.DTOs;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;

namespace WesNews.Application.Interfaces.Repositories;

public interface INewsArticleRepository
{
    Task<PagedResult<NewsArticle>> GetPagedAsync(NewsQuery query, CancellationToken cancellationToken = default);
    Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpsertRangeAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsArticle>> GetRecentByCategoryAsync(Category category, int hours, int limit, CancellationToken cancellationToken = default);
    Task ClearFeaturedByCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task SetFeaturedAsync(IEnumerable<Guid> articleIds, CancellationToken cancellationToken = default);
}
