using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;

namespace WesNews.Application.Services;

public class NewsService(INewsArticleRepository repository)
{
    public async Task<PagedResult<NewsArticleDto>> GetPagedAsync(NewsQuery query, CancellationToken cancellationToken = default)
    {
        PagedResult<NewsArticle> result = await repository.GetPagedAsync(query, cancellationToken);

        IReadOnlyList<NewsArticleDto> dtos = result.Items
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();

        return new PagedResult<NewsArticleDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }

    public async Task<bool> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await repository.MarkAsReadAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await repository.DeleteAsync(id, cancellationToken);
    }

    private static NewsArticleDto MapToDto(NewsArticle article)
    {
        return new NewsArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Summary = article.Summary,
            Url = article.Url,
            ImageUrl = article.ImageUrl,
            PublishedAt = article.PublishedAt,
            IsRead = article.IsRead,
            Category = article.FeedSource.Category,
            FeedSourceName = article.FeedSource.Name
        };
    }
}
