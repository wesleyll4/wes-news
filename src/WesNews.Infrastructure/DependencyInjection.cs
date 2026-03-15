using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Resend;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Infrastructure.BackgroundServices;
using WesNews.Infrastructure.Configuration;
using WesNews.Infrastructure.Data;
using WesNews.Infrastructure.Repositories;
using WesNews.Infrastructure.Seed;
using WesNews.Infrastructure.Services;

namespace WesNews.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=wesnews.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        services.AddScoped<IFeedSourceRepository, FeedSourceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFeedAggregatorService, FeedAggregatorService>();
        services.AddScoped<IDigestEmailService, DigestEmailService>();
        services.AddScoped<IAiCuratorService, GeminiCuratorService>();
        services.AddScoped<FeedSeeder>();

        services.Configure<DigestEmailOptions>(configuration.GetSection("DigestEmail"));
        services.Configure<GeminiOptions>(configuration.GetSection("Gemini"));

        services.AddHttpClient();
        services.AddHttpClient("FeedAggregator", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddStandardResilienceHandler();
        services.AddHttpClient("Gemini", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = configuration["RESEND_APITOKEN"] ?? string.Empty;
        });
        services.AddTransient<IResend, ResendClient>();

        services.AddHostedService<BackgroundFetchService>();

        GeminiOptions geminiOptions = configuration.GetSection("Gemini").Get<GeminiOptions>() ?? new GeminiOptions();
        string digestCron = configuration["DigestEmail:CronExpression"] ?? "0 0 7 * * ?";

        services.AddQuartz(q =>
        {
            JobKey digestJobKey = new JobKey("DigestSchedulerJob");
            q.AddJob<DigestSchedulerJob>(opts => opts.WithIdentity(digestJobKey));
            q.AddTrigger(opts => opts
                .ForJob(digestJobKey)
                .WithIdentity("DigestSchedulerJob-trigger")
                .WithCronSchedule(digestCron));

            JobKey curatorJobKey = new JobKey("CuratorSchedulerJob");
            q.AddJob<CuratorSchedulerJob>(opts => opts.WithIdentity(curatorJobKey));
            q.AddTrigger(opts => opts
                .ForJob(curatorJobKey)
                .WithIdentity("CuratorSchedulerJob-morning")
                .WithCronSchedule(geminiOptions.MorningCron));
            q.AddTrigger(opts => opts
                .ForJob(curatorJobKey)
                .WithIdentity("CuratorSchedulerJob-afternoon")
                .WithCronSchedule(geminiOptions.AfternoonCron));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}
