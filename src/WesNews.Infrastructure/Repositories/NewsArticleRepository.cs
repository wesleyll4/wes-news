using Microsoft.EntityFrameworkCore;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;
using WesNews.Infrastructure.Data;

namespace WesNews.Infrastructure.Repositories;

public class NewsArticleRepository(AppDbContext context) : INewsArticleRepository
{
    public async Task<PagedResult<NewsArticle>> GetPagedAsync(NewsQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<NewsArticle> queryable = context.NewsArticles
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
            .OrderByDescending(a => a.IsFeatured)
            .ThenByDescending(a => a.PublishedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<NewsArticle>(items.AsReadOnly(), totalCount, query.Page, query.PageSize);
    }

    public async Task<NewsArticle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.NewsArticles
            .Include(a => a.FeedSource)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        int affected = await context.NewsArticles
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true), cancellationToken);

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        int affected = await context.NewsArticles
            .Where(a => a.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return affected > 0;
    }

    public async Task UpsertRangeAsync(IEnumerable<NewsArticle> articles, CancellationToken cancellationToken = default)
    {
        foreach (NewsArticle article in articles)
        {
            bool exists = await context.NewsArticles
                .AnyAsync(a => a.Url == article.Url, cancellationToken);

            if (!exists)
            {
                await context.NewsArticles.AddAsync(article, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NewsArticle>> GetRecentByCategoryAsync(
        Category category,
        int hours,
        int limit,
        CancellationToken cancellationToken = default)
    {
        DateTime since = DateTime.UtcNow.AddHours(-hours);

        List<NewsArticle> items = await context.NewsArticles
            .Include(a => a.FeedSource)
            .AsNoTracking()
            .Where(a => a.FeedSource.Category == category && a.PublishedAt >= since)
            .OrderByDescending(a => a.PublishedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }

    public async Task ClearFeaturedByCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        await context.NewsArticles
            .Where(a => a.IsFeatured && a.FeedSource.Category == category)
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.IsFeatured, false)
                      .SetProperty(a => a.FeaturedAt, (DateTime?)null),
                cancellationToken);
    }

    public async Task SetFeaturedAsync(IEnumerable<Guid> articleIds, CancellationToken cancellationToken = default)
    {
        List<Guid> ids = articleIds.ToList();
        DateTime now = DateTime.UtcNow;

        await context.NewsArticles
            .Where(a => ids.Contains(a.Id))
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.IsFeatured, true)
                      .SetProperty(a => a.FeaturedAt, now),
                cancellationToken);
    }
}
