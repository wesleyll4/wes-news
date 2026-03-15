using Microsoft.Extensions.Logging;
using Quartz;
using WesNews.Application.Interfaces.Services;

namespace WesNews.Infrastructure.Services;

[DisallowConcurrentExecution]
public class CuratorSchedulerJob(IAiCuratorService curatorService, ILogger<CuratorSchedulerJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Running AI curation job");
        await curatorService.CurateAsync(context.CancellationToken);
        logger.LogInformation("AI curation job completed");
    }
}
