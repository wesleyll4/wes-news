using Microsoft.EntityFrameworkCore;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Data;

namespace WesNews.Infrastructure.Repositories;

public class FeedSourceRepository : IFeedSourceRepository
{
    private readonly AppDbContext _context;

    public FeedSourceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FeedSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<FeedSource> feeds = await _context.FeedSources
            .AsNoTracking()
            .OrderBy(f => f.Category)
            .ThenBy(f => f.Name)
            .ToListAsync(cancellationToken);

        return feeds.AsReadOnly();
    }

    public async Task<IReadOnlyList<FeedSource>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        List<FeedSource> feeds = await _context.FeedSources
            .AsNoTracking()
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken);

        return feeds.AsReadOnly();
    }

    public async Task<FeedSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FeedSources.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return await _context.FeedSources.AnyAsync(f => f.Url == url, cancellationToken);
    }

    public async Task<FeedSource> AddAsync(FeedSource feedSource, CancellationToken cancellationToken = default)
    {
        await _context.FeedSources.AddAsync(feedSource, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return feedSource;
    }

    public async Task<bool> UpdateAsync(FeedSource feedSource, CancellationToken cancellationToken = default)
    {
        _context.FeedSources.Update(feedSource);
        int affected = await _context.SaveChangesAsync(cancellationToken);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        int affected = await _context.FeedSources
            .Where(f => f.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return affected > 0;
    }
}
