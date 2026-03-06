using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Data;

namespace WesNews.IntegrationTests;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            RemoveService<DbContextOptions<AppDbContext>>(services);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            RemoveService<IDigestEmailService>(services);
            IDigestEmailService emailStub = Substitute.For<IDigestEmailService>();
            emailStub.BuildPreviewHtml(Arg.Any<IEnumerable<NewsArticle>>())
                .Returns("<html><body>Digest Preview</body></html>");
            services.AddScoped(_ => emailStub);

            RemoveHostedService<WesNews.Infrastructure.BackgroundServices.BackgroundFetchService>(services);
        });

        builder.UseEnvironment("Testing");
    }

    private static void RemoveService<TService>(IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveHostedService<TService>(IServiceCollection services)
    {
        ServiceDescriptor? descriptor = services.SingleOrDefault(
            d => d.ImplementationType == typeof(TService));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
