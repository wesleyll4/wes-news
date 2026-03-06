using WesNews.Domain.Entities;

namespace WesNews.Application.Interfaces.Services;

public interface IFeedAggregatorService
{
    Task FetchAndSaveAsync(FeedSource feedSource, CancellationToken cancellationToken = default);
}
