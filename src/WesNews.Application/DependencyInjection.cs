using Microsoft.Extensions.DependencyInjection;
using WesNews.Application.Services;

namespace WesNews.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<NewsService>();
        services.AddScoped<FeedService>();
        services.AddScoped<DigestService>();
        return services;
    }
}
