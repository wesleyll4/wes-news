using Microsoft.Extensions.Logging;
using Quartz;
using WesNews.Application.Services;

namespace WesNews.Infrastructure.Services;

[DisallowConcurrentExecution]
public class DigestSchedulerJob(DigestService digestService, ILogger<DigestSchedulerJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Running daily digest job");
        await digestService.SendAsync(context.CancellationToken);
        logger.LogInformation("Daily digest job completed");
    }
}
