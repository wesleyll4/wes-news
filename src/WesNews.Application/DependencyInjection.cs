using Microsoft.Extensions.DependencyInjection;
using WesNews.Application.Interfaces.Services;
using WesNews.Application.Services;

namespace WesNews.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<NewsService>();
        services.AddScoped<FeedService>();
        services.AddScoped<DigestService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
