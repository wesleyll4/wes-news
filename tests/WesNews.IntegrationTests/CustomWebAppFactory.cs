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

            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            
            services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test").RequireAuthenticatedUser().Build());
        });

        builder.UseEnvironment("Testing");
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
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

public class TestAuthHandler(
    Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
    Microsoft.Extensions.Logging.ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder)
    : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        System.Security.Claims.Claim[] claims = [new(System.Security.Claims.ClaimTypes.Name, "TestUser"), new(System.Security.Claims.ClaimTypes.Role, "Admin")];
        System.Security.Claims.ClaimsIdentity identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
        System.Security.Claims.ClaimsPrincipal principal = new System.Security.Claims.ClaimsPrincipal(identity);
        Microsoft.AspNetCore.Authentication.AuthenticationTicket ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");
        
        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}
