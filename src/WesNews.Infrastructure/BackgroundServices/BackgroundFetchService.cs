using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Infrastructure.BackgroundServices;

public class BackgroundFetchService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundFetchService> _logger;
    private static readonly TimeSpan FetchInterval = TimeSpan.FromHours(2);

    public BackgroundFetchService(IServiceScopeFactory scopeFactory, ILogger<BackgroundFetchService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundFetchService started");

        await FetchAllFeedsAsync(stoppingToken);

        using PeriodicTimer timer = new PeriodicTimer(FetchInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FetchAllFeedsAsync(stoppingToken);
        }
    }

    private async Task FetchAllFeedsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();

        IFeedSourceRepository feedRepository = scope.ServiceProvider.GetRequiredService<IFeedSourceRepository>();
        IFeedAggregatorService aggregatorService = scope.ServiceProvider.GetRequiredService<IFeedAggregatorService>();

        IReadOnlyList<FeedSource> activeFeeds = await feedRepository.GetActiveAsync(cancellationToken);

        _logger.LogInformation("Fetching {Count} active feeds", activeFeeds.Count);

        foreach (FeedSource feed in activeFeeds)
        {
            await aggregatorService.FetchAndSaveAsync(feed, cancellationToken);
        }
    }
}
