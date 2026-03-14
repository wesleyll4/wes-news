using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Infrastructure.BackgroundServices;

public class BackgroundFetchService(IServiceScopeFactory scopeFactory, ILogger<BackgroundFetchService> logger) : BackgroundService
{
    private static readonly TimeSpan FetchInterval = TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BackgroundFetchService started");

        await FetchAllFeedsAsync(stoppingToken);

        using PeriodicTimer timer = new PeriodicTimer(FetchInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FetchAllFeedsAsync(stoppingToken);
        }
    }

    private async Task FetchAllFeedsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        IFeedSourceRepository feedRepository = scope.ServiceProvider.GetRequiredService<IFeedSourceRepository>();

        IReadOnlyList<FeedSource> activeFeeds = await feedRepository.GetActiveAsync(cancellationToken);

        logger.LogInformation("Fetching {Count} active feeds", activeFeeds.Count);

        using SemaphoreSlim semaphore = new SemaphoreSlim(5, 5);
        List<Task> fetchTasks = new List<Task>();

        foreach (FeedSource feed in activeFeeds)
        {
            await semaphore.WaitAsync(cancellationToken);

            Task task = Task.Run(async () =>
            {
                try
                {
                    using IServiceScope localScope = scopeFactory.CreateScope();
                    IFeedAggregatorService localAggregator = localScope.ServiceProvider.GetRequiredService<IFeedAggregatorService>();

                    await localAggregator.FetchAndSaveAsync(feed, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Background error fetching feed {FeedName} ({Url})", feed.Name, feed.Url);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);

            fetchTasks.Add(task);
        }

        await Task.WhenAll(fetchTasks);
    }
}
