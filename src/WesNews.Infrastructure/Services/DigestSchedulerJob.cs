using Microsoft.Extensions.Logging;
using Quartz;
using WesNews.Application.Services;

namespace WesNews.Infrastructure.Services;

[DisallowConcurrentExecution]
public class DigestSchedulerJob : IJob
{
    private readonly DigestService _digestService;
    private readonly ILogger<DigestSchedulerJob> _logger;

    public DigestSchedulerJob(DigestService digestService, ILogger<DigestSchedulerJob> logger)
    {
        _digestService = digestService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Running daily digest job");
        await _digestService.SendAsync(context.CancellationToken);
        _logger.LogInformation("Daily digest job completed");
    }
}
