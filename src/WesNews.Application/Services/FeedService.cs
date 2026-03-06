using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;

namespace WesNews.Application.Services;

public class FeedService
{
    private readonly IFeedSourceRepository _repository;

    public FeedService(IFeedSourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FeedSourceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FeedSource> feeds = await _repository.GetAllAsync(cancellationToken);
        return feeds.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<FeedSourceDto> AddAsync(CreateFeedSourceRequest request, CancellationToken cancellationToken = default)
    {
        bool exists = await _repository.ExistsByUrlAsync(request.Url, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"A feed with URL '{request.Url}' already exists.");
        }

        FeedSource feedSource = new FeedSource
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            Category = request.Category,
            IsActive = true
        };

        FeedSource created = await _repository.AddAsync(feedSource, cancellationToken);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateFeedSourceRequest request, CancellationToken cancellationToken = default)
    {
        FeedSource? feed = await _repository.GetByIdAsync(id, cancellationToken);

        if (feed is null)
        {
            return false;
        }

        if (request.Name is not null)
        {
            feed.Name = request.Name;
        }

        if (request.IsActive.HasValue)
        {
            feed.IsActive = request.IsActive.Value;
        }

        if (request.Category.HasValue)
        {
            feed.Category = request.Category.Value;
        }

        return await _repository.UpdateAsync(feed, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    private static FeedSourceDto MapToDto(FeedSource feed)
    {
        return new FeedSourceDto
        {
            Id = feed.Id,
            Name = feed.Name,
            Url = feed.Url,
            Category = feed.Category,
            IsActive = feed.IsActive,
            LastFetchedAt = feed.LastFetchedAt
        };
    }
}
