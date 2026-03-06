using WesNews.Domain.Entities;

namespace WesNews.Application.Interfaces.Repositories;

public interface IFeedSourceRepository
{
    Task<IReadOnlyList<FeedSource>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeedSource>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<FeedSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<FeedSource> AddAsync(FeedSource feedSource, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(FeedSource feedSource, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
