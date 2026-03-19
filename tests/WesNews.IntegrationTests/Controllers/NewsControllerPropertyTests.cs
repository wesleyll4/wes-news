using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Data;
using WesNews.IntegrationTests.Helpers;

namespace WesNews.IntegrationTests.Controllers;

/// <summary>
/// Feature: public-access, Property 3: GET /api/news returns 200 regardless of auth
/// Validates: Requirements 2.1, 2.2
/// </summary>
public class NewsControllerPublicAccessPropertyTests : IClassFixture<PublicAccessWebFactory>
{
    private readonly PublicAccessWebFactory _factory;

    public NewsControllerPublicAccessPropertyTests(PublicAccessWebFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// **Validates: Requirements 2.1, 2.2**
    /// For any boolean hasToken, GET /api/news must return HTTP 200 with a paged result,
    /// regardless of whether the request includes a valid JWT token or not.
    /// Tag: Feature: public-access, Property 3: GET /api/news returns 200 regardless of auth
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "GET /api/news retorna 200 independente de autenticação")]
    public bool GetNews_ReturnsOk_ForAnyAuthState(bool hasToken)
    {
        return Task.Run(async () =>
        {
            HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            if (hasToken)
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
            }
            // else: no Authorization header — anonymous request

            HttpResponseMessage response = await client.GetAsync("/api/news");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Expected 200 OK but got {response.StatusCode} (hasToken={hasToken}). Body: {content}");
            }

            PagedResult<NewsArticleDto>? body =
                await response.Content.ReadFromJsonAsync<PagedResult<NewsArticleDto>>();

            if (body is null)
                throw new Exception("Response body was null");

            return true;
        }).GetAwaiter().GetResult();
    }
}

/// <summary>
/// WebApplicationFactory for public-access integration tests.
/// Supports both authenticated (Test scheme) and anonymous requests.
/// Does NOT set a fallback authorization policy so anonymous requests are allowed.
/// </summary>
public class PublicAccessWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection;

    public PublicAccessWebFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            ServiceDescriptor? emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDigestEmailService));
            if (emailDescriptor is not null)
                services.Remove(emailDescriptor);

            IDigestEmailService emailStub = Substitute.For<IDigestEmailService>();
            emailStub.BuildPreviewHtml(Arg.Any<IEnumerable<NewsArticle>>())
                .Returns("<html><body>Digest Preview</body></html>");
            services.AddScoped(_ => emailStub);

            ServiceDescriptor? bgDescriptor = services.SingleOrDefault(
                d => d.ImplementationType == typeof(WesNews.Infrastructure.BackgroundServices.BackgroundFetchService));
            if (bgDescriptor is not null)
                services.Remove(bgDescriptor);

            // Register the Test auth scheme so authenticated requests work,
            // but do NOT set a fallback policy — anonymous requests must be allowed.
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, PublicAccessTestAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await TestDbSeeder.SeedAsync(context);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        _connection.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

/// <summary>
/// Auth handler for public-access tests. Returns a standard authenticated user principal.
/// </summary>
public class PublicAccessTestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Only authenticate if an Authorization header is present.
        // Returning NoResult for anonymous requests lets [Authorize] challenge them (→ 401).
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "User")
        ];

        ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Feature: public-access, Property 4: unreadOnly ignored for anonymous requests
/// Validates: Requirements 2.3
/// </summary>
public class NewsControllerUnreadOnlyAnonymousPropertyTests : IClassFixture<PublicAccessWebFactory>
{
    private readonly PublicAccessWebFactory _factory;

    public NewsControllerUnreadOnlyAnonymousPropertyTests(PublicAccessWebFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// **Validates: Requirements 2.3**
    /// For any boolean unreadOnly, GET /api/news?unreadOnly={unreadOnly} without a token
    /// must return the same set of articles as GET /api/news?unreadOnly=false without a token.
    /// Tag: Feature: public-access, Property 4: unreadOnly ignored for anonymous requests
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "unreadOnly ignorado para requisições anônimas")]
    public bool GetNews_Anonymous_UnreadOnlyIsIgnored(bool unreadOnly)
    {
        return Task.Run(async () =>
        {
            HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            // No Authorization header — anonymous request

            HttpResponseMessage responseWithFlag = await client.GetAsync(
                $"/api/news?unreadOnly={unreadOnly.ToString().ToLower()}");
            HttpResponseMessage responseBaseline = await client.GetAsync(
                "/api/news?unreadOnly=false");

            if (responseWithFlag.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Expected 200 OK but got {responseWithFlag.StatusCode} (unreadOnly={unreadOnly})");

            if (responseBaseline.StatusCode != HttpStatusCode.OK)
                throw new Exception($"Expected 200 OK but got {responseBaseline.StatusCode} (baseline)");

            PagedResult<NewsArticleDto>? bodyWithFlag =
                await responseWithFlag.Content.ReadFromJsonAsync<PagedResult<NewsArticleDto>>();
            PagedResult<NewsArticleDto>? bodyBaseline =
                await responseBaseline.Content.ReadFromJsonAsync<PagedResult<NewsArticleDto>>();

            if (bodyWithFlag is null || bodyBaseline is null)
                throw new Exception("One or both response bodies were null");

            if (bodyWithFlag.TotalCount != bodyBaseline.TotalCount)
                throw new Exception(
                    $"TotalCount mismatch: unreadOnly={unreadOnly} returned {bodyWithFlag.TotalCount}, baseline returned {bodyBaseline.TotalCount}");

            HashSet<Guid> idsWithFlag = bodyWithFlag.Items.Select(a => a.Id).ToHashSet();
            HashSet<Guid> idsBaseline = bodyBaseline.Items.Select(a => a.Id).ToHashSet();

            if (!idsWithFlag.SetEquals(idsBaseline))
                throw new Exception(
                    $"Article ID sets differ: unreadOnly={unreadOnly} returned [{string.Join(", ", idsWithFlag)}], baseline returned [{string.Join(", ", idsBaseline)}]");

            return true;
        }).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Feature: public-access, Property 5: write ops on /api/news require auth
/// Validates: Requirements 2.4, 2.5
/// </summary>
public class NewsControllerWriteOpsRequireAuthPropertyTests : IClassFixture<PublicAccessWebFactory>
{
    private readonly PublicAccessWebFactory _factory;

    public NewsControllerWriteOpsRequireAuthPropertyTests(PublicAccessWebFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// **Validates: Requirements 2.4, 2.5**
    /// For any Guid id, PATCH /api/news/{id}/read and DELETE /api/news/{id} without
    /// an Authorization header must both return HTTP 401 Unauthorized.
    /// Tag: Feature: public-access, Property 5: write ops on /api/news require auth
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Operações de escrita em /api/news sem token retornam 401")]
    public bool NewsWriteOps_WithoutToken_Return401(Guid id)
    {
        return Task.Run(async () =>
        {
            HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            // No Authorization header — anonymous request

            HttpResponseMessage patchResponse = await client.PatchAsync(
                $"/api/news/{id}/read", null);

            if (patchResponse.StatusCode != HttpStatusCode.Unauthorized)
                throw new Exception(
                    $"Expected 401 Unauthorized for PATCH /api/news/{id}/read but got {patchResponse.StatusCode}");

            HttpResponseMessage deleteResponse = await client.DeleteAsync(
                $"/api/news/{id}");

            if (deleteResponse.StatusCode != HttpStatusCode.Unauthorized)
                throw new Exception(
                    $"Expected 401 Unauthorized for DELETE /api/news/{id} but got {deleteResponse.StatusCode}");

            return true;
        }).GetAwaiter().GetResult();
    }
}
