using Microsoft.EntityFrameworkCore;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Data;

namespace WesNews.Infrastructure.Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    private readonly AppDbContext _context;

    public NewsArticleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<NewsArticle>> GetPagedAsync(NewsQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<NewsArticle> queryable = _context.NewsArticles
            .Include(a => a.FeedSource)
            .AsNoTracking();

        if (query.Category.HasValue)
        {
            queryable = queryable.Where(a => a.FeedSource.Category == query.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string searchLower = query.Search.ToLower();
            queryable = queryable.Where(a =>
                a.Title.ToLower().Contains(searchLower) ||
                a.Summary.ToLower().Contains(searchLower));
        }

        if (query.UnreadOnly)
        {
            queryable = queryable.Where(a => !a.IsRead);
        }

        int totalCount = await queryable.CountAsync(cancellationToken);

        List<NewsArticle> items = await queryable
            .OrderByDescending(a => a.PublishedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<NewsArticle>(items.AsReadOnly(), totalCount, query.Page, query.PageSize);
    }

    public async Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NewsArticles
            .Include(a => a.FeedSource)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        int affected = await _context.NewsArticles
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true), cancellationToken);

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        int affected = await _context.NewsArticles
            .Where(a => a.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return affected > 0;
    }

    public async Task UpsertRangeAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default)
    {
        foreach (NewsArticle article in articles)
        {
            bool exists = await _context.NewsArticles
                .AnyAsync(a => a.Url == article.Url, cancellationToken);

            if (!exists)
            {
                await _context.NewsArticles.AddAsync(article, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
