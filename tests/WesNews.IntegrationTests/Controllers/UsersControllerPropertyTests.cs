using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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

namespace WesNews.IntegrationTests.Controllers;

/// <summary>
/// Feature: user-digest-preference, Property 4: PATCH atualiza e retorna o valor correto
/// Validates: Requirements 2.2
/// </summary>
public class UsersControllerPropertyTests : IClassFixture<DigestPreferenceWebFactory>
{
    private readonly DigestPreferenceWebFactory _factory;

    public UsersControllerPropertyTests(DigestPreferenceWebFactory factory)
    {
        _factory = factory;
    }

    private static User CreateTestUser(Guid userId) => new User
    {
        Id = userId,
        Username = $"u{userId:N}",
        Email = $"u{userId:N}@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
        FullName = "Test User",
        Role = "User",
        DigestEnabled = false,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// **Validates: Requirements 2.2**
    /// For any authenticated user and any boolean value v, sending
    /// PATCH /api/users/me/digest-preference with { "digestEnabled": v }
    /// must return { "digestEnabled": v } and the DB must reflect the same value.
    /// Tag: Feature: user-digest-preference, Property 4: PATCH atualiza e retorna o valor correto
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "PATCH atualiza e retorna o valor correto")]
    public bool PatchDigestPreference_UpdatesAndReturnsCorrectValue(bool digestEnabled)
    {
        return Task.Run(async () =>
        {
            Guid userId = Guid.NewGuid();

            // Update the mutable holder so the auth handler uses this user's ID
            _factory.CurrentUserId = userId;

            // Seed the user into the DB
            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Users.Add(CreateTestUser(userId));
                await context.SaveChangesAsync();
            }

            HttpClient client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            HttpResponseMessage response = await client.PatchAsJsonAsync(
                "/api/users/me/digest-preference",
                new { digestEnabled }
            );

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Expected 200 OK but got {response.StatusCode}. Body: {content}");
            }

            DigestPreferenceResponse? body = await response.Content
                .ReadFromJsonAsync<DigestPreferenceResponse>();

            if (body is null)
                throw new Exception("Response body was null");

            if (body.DigestEnabled != digestEnabled)
                throw new Exception($"Response DigestEnabled={body.DigestEnabled} but expected {digestEnabled}");

            // Verify DB reflects the same value
            using IServiceScope verifyScope = _factory.Services.CreateScope();
            AppDbContext verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            User? dbUser = await verifyContext.Users.FindAsync(userId);

            if (dbUser is null)
                throw new Exception($"User {userId} not found in DB after PATCH");

            if (dbUser.DigestEnabled != digestEnabled)
                throw new Exception($"DB DigestEnabled={dbUser.DigestEnabled} but expected {digestEnabled}");

            return true;
        }).GetAwaiter().GetResult();
    }
}

/// <summary>
/// WebApplicationFactory for digest preference integration tests.
/// Uses a shared in-memory SQLite database and a mutable user ID holder.
/// </summary>
public class DigestPreferenceWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private readonly MutableUserIdHolder _userIdHolder;

    public DigestPreferenceWebFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _userIdHolder = new MutableUserIdHolder();
    }

    public Guid CurrentUserId
    {
        get => _userIdHolder.UserId;
        set => _userIdHolder.UserId = value;
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

            services.AddSingleton(_userIdHolder);

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, MutableUserIdAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser().Build());
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
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
/// Mutable holder for the current user ID, shared across all requests in a test run.
/// </summary>
public sealed class MutableUserIdHolder
{
    public Guid UserId { get; set; } = Guid.Empty;
}

/// <summary>
/// Feature: user-digest-preference, Property 5: Payloads inválidos retornam HTTP 400
/// Validates: Requirements 2.4
/// </summary>
public class UsersControllerInvalidPayloadPropertyTests : IClassFixture<DigestPreferenceWebFactory>
{
    private readonly DigestPreferenceWebFactory _factory;

    public UsersControllerInvalidPayloadPropertyTests(DigestPreferenceWebFactory factory)
    {
        _factory = factory;
    }

    private static User CreateTestUser(Guid userId) => new User
    {
        Id = userId,
        Username = $"u{userId:N}",
        Email = $"u{userId:N}@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
        FullName = "Test User",
        Role = "User",
        DigestEnabled = false,
        CreatedAt = DateTime.UtcNow
    };

    private HttpClient CreateAuthenticatedClient(Guid userId)
    {
        _factory.CurrentUserId = userId;
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
        return client;
    }

    private async Task SeedUserAsync(Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await context.Users.FindAsync(userId) is null)
        {
            context.Users.Add(CreateTestUser(userId));
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// **Validates: Requirements 2.4**
    /// For any payload where digestEnabled is a string (not a boolean),
    /// the endpoint must return HTTP 400.
    /// Tag: Feature: user-digest-preference, Property 5: Payloads inválidos retornam HTTP 400
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Payload com digestEnabled string retorna 400")]
    public bool InvalidPayload_StringDigestEnabled_Returns400(NonEmptyString value)
    {
        return Task.Run(async () =>
        {
            Guid userId = Guid.NewGuid();
            await SeedUserAsync(userId);
            HttpClient client = CreateAuthenticatedClient(userId);

            string json = $"{{\"digestEnabled\":\"{value.Get}\"}}";
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PatchAsync("/api/users/me/digest-preference", content);

            return response.StatusCode == HttpStatusCode.BadRequest;
        }).GetAwaiter().GetResult();
    }

    /// <summary>
    /// **Validates: Requirements 2.4**
    /// For any payload where digestEnabled is a number (not a boolean),
    /// the endpoint must return HTTP 400.
    /// Tag: Feature: user-digest-preference, Property 5: Payloads inválidos retornam HTTP 400
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Payload com digestEnabled número retorna 400")]
    public bool InvalidPayload_NumberDigestEnabled_Returns400(int value)
    {
        return Task.Run(async () =>
        {
            Guid userId = Guid.NewGuid();
            await SeedUserAsync(userId);
            HttpClient client = CreateAuthenticatedClient(userId);

            string json = $"{{\"digestEnabled\":{value}}}";
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PatchAsync("/api/users/me/digest-preference", content);

            return response.StatusCode == HttpStatusCode.BadRequest;
        }).GetAwaiter().GetResult();
    }

    /// <summary>
    /// **Validates: Requirements 2.4**
    /// A payload with digestEnabled set to null must return HTTP 400.
    /// Tag: Feature: user-digest-preference, Property 5: Payloads inválidos retornam HTTP 400
    /// </summary>
    [Fact(DisplayName = "Payload com digestEnabled null retorna 400")]
    public async Task InvalidPayload_NullDigestEnabled_Returns400()
    {
        Guid userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        HttpClient client = CreateAuthenticatedClient(userId);

        string json = "{\"digestEnabled\":null}";
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PatchAsync("/api/users/me/digest-preference", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// **Validates: Requirements 2.4**
    /// An empty body must return HTTP 400.
    /// Tag: Feature: user-digest-preference, Property 5: Payloads inválidos retornam HTTP 400
    /// </summary>
    [Fact(DisplayName = "Corpo vazio retorna 400")]
    public async Task InvalidPayload_EmptyBody_Returns400()
    {
        Guid userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        HttpClient client = CreateAuthenticatedClient(userId);

        StringContent content = new StringContent("", Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PatchAsync("/api/users/me/digest-preference", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// **Validates: Requirements 2.4**
    /// A non-JSON body must return HTTP 400.
    /// Tag: Feature: user-digest-preference, Property 5: Payloads inválidos retornam HTTP 400
    /// </summary>
    [Fact(DisplayName = "Corpo não-JSON retorna 400")]
    public async Task InvalidPayload_NonJsonBody_Returns400()
    {
        Guid userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        HttpClient client = CreateAuthenticatedClient(userId);

        StringContent content = new StringContent("not-json-at-all", Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PatchAsync("/api/users/me/digest-preference", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

/// <summary>
/// Auth handler that injects the current user ID as ClaimTypes.NameIdentifier.
/// </summary>
public class MutableUserIdAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    System.Text.Encodings.Web.UrlEncoder encoder,
    MutableUserIdHolder userIdHolder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userIdHolder.UserId.ToString()),
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
/// WebApplicationFactory for delete account integration tests.
/// Uses a shared in-memory SQLite database and a mutable user ID holder.
/// </summary>
public class DeleteAccountWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private readonly MutableUserIdHolder _userIdHolder;

    public DeleteAccountWebFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _userIdHolder = new MutableUserIdHolder();
    }

    public Guid CurrentUserId
    {
        get => _userIdHolder.UserId;
        set => _userIdHolder.UserId = value;
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

            services.AddSingleton(_userIdHolder);

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, MutableUserIdAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser().Build());
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
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
/// Feature: delete-account, Property 3: Endpoint retorna 204 para usuário autenticado existente
/// Validates: Requirements 1.2
/// </summary>
public class DeleteAccountEndpointReturns204PropertyTests : IClassFixture<DeleteAccountWebFactory>
{
    private readonly DeleteAccountWebFactory _factory;

    public DeleteAccountEndpointReturns204PropertyTests(DeleteAccountWebFactory factory)
    {
        _factory = factory;
    }

    private static User CreateTestUser(Guid userId) => new User
    {
        Id = userId,
        Username = $"u{userId:N}",
        Email = $"u{userId:N}@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
        FullName = "Test User",
        Role = "User",
        DigestEnabled = false,
        CreatedAt = DateTime.UtcNow
    };

    // Feature: delete-account, Property 3: Endpoint retorna 204 para usuário autenticado existente
    /// <summary>
    /// **Validates: Requirements 1.2**
    /// For any existing userId with a valid JWT token,
    /// DELETE /api/users/me must return HTTP 204 No Content.
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "DELETE /api/users/me retorna 204 para usuário autenticado existente")]
    public bool DeleteMe_AuthenticatedExistingUser_Returns204()
    {
        return Task.Run(async () =>
        {
            Guid userId = Guid.NewGuid();
            _factory.CurrentUserId = userId;

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Users.Add(CreateTestUser(userId));
                await context.SaveChangesAsync();
            }

            HttpClient client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            HttpResponseMessage response = await client.DeleteAsync("/api/users/me");

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Expected 204 NoContent but got {response.StatusCode}. Body: {content}");
            }

            return true;
        }).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Minimal WebApplicationFactory for testing unauthenticated requests (no test auth override).
/// Uses in-memory SQLite and removes background services.
/// </summary>
public class NoAuthWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection;

    public NoAuthWebFactory()
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
            services.AddScoped(_ => emailStub);

            ServiceDescriptor? bgDescriptor = services.SingleOrDefault(
                d => d.ImplementationType == typeof(WesNews.Infrastructure.BackgroundServices.BackgroundFetchService));
            if (bgDescriptor is not null)
                services.Remove(bgDescriptor);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
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
/// Feature: delete-account, Property 4: Endpoint retorna 401 para token inválido
/// Validates: Requirements 1.3, 1.5
/// </summary>
public class DeleteAccountEndpointReturns401PropertyTests : IClassFixture<NoAuthWebFactory>
{
    private readonly NoAuthWebFactory _factory;

    public DeleteAccountEndpointReturns401PropertyTests(NoAuthWebFactory factory)
    {
        _factory = factory;
    }

    // Feature: delete-account, Property 4: Endpoint retorna 401 para token inválido
    /// <summary>
    /// **Validates: Requirements 1.3, 1.5**
    /// For any request to DELETE /api/users/me without a valid JWT token,
    /// the API must return HTTP 401 Unauthorized.
    /// </summary>
    [Fact(DisplayName = "DELETE /api/users/me sem token retorna 401")]
    public async Task DeleteMe_WithoutToken_Returns401()
    {
        HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        // No Authorization header — should be rejected by JWT middleware

        HttpResponseMessage response = await client.DeleteAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
