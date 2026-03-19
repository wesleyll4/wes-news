using System.Net;
using System.Net.Http.Json;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using WesNews.Application.DTOs;

namespace WesNews.IntegrationTests.Controllers;

/// <summary>
/// Feature: public-access, Property 6: GET /api/feeds returns 200 without auth
/// Validates: Requirements 2.6
/// </summary>
public class FeedsControllerPublicAccessPropertyTests : IClassFixture<PublicAccessWebFactory>
{
    private readonly PublicAccessWebFactory _factory;

    public FeedsControllerPublicAccessPropertyTests(PublicAccessWebFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// **Validates: Requirements 2.6**
    /// For any boolean hasToken, GET /api/feeds must return HTTP 200 with the list of feed sources,
    /// regardless of whether the request includes a valid JWT token or not.
    /// Tag: Feature: public-access, Property 6: GET /api/feeds returns 200 without auth
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "GET /api/feeds retorna 200 sem autenticação")]
    public bool GetFeeds_ReturnsOk_ForAnyAuthState(bool hasToken)
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

            HttpResponseMessage response = await client.GetAsync("/api/feeds");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string content = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Expected 200 OK but got {response.StatusCode} (hasToken={hasToken}). Body: {content}");
            }

            IReadOnlyList<FeedSourceDto>? body =
                await response.Content.ReadFromJsonAsync<IReadOnlyList<FeedSourceDto>>();

            if (body is null)
                throw new Exception("Response body was null");

            return true;
        }).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Feature: public-access, Property 7: write ops on /api/feeds require Admin
/// Validates: Requirements 2.7, 2.8
/// </summary>
public class FeedsControllerWriteOpsRequireAdminPropertyTests : IClassFixture<PublicAccessWebFactory>
{
    private readonly PublicAccessWebFactory _factory;

    public FeedsControllerWriteOpsRequireAdminPropertyTests(PublicAccessWebFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// **Validates: Requirements 2.7**
    /// For any POST, PUT or DELETE to /api/feeds without an Authorization header,
    /// the server must return HTTP 401 Unauthorized.
    /// Tag: Feature: public-access, Property 7: write ops on /api/feeds require Admin
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "POST/PUT/DELETE /api/feeds sem token retornam 401")]
    public bool FeedsWriteOps_WithoutToken_Return401(Guid id)
    {
        return Task.Run(async () =>
        {
            HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            // No Authorization header — anonymous request

            HttpResponseMessage postResponse = await client.PostAsJsonAsync(
                "/api/feeds",
                new CreateFeedSourceRequest { Name = "Test", Url = $"https://test-{id}.example.com/feed" });

            if (postResponse.StatusCode != HttpStatusCode.Unauthorized)
                throw new Exception(
                    $"Expected 401 Unauthorized for POST /api/feeds but got {postResponse.StatusCode}");

            HttpResponseMessage putResponse = await client.PutAsJsonAsync(
                $"/api/feeds/{id}",
                new UpdateFeedSourceRequest { Name = "Updated" });

            if (putResponse.StatusCode != HttpStatusCode.Unauthorized)
                throw new Exception(
                    $"Expected 401 Unauthorized for PUT /api/feeds/{id} but got {putResponse.StatusCode}");

            HttpResponseMessage deleteResponse = await client.DeleteAsync(
                $"/api/feeds/{id}");

            if (deleteResponse.StatusCode != HttpStatusCode.Unauthorized)
                throw new Exception(
                    $"Expected 401 Unauthorized for DELETE /api/feeds/{id} but got {deleteResponse.StatusCode}");

            return true;
        }).GetAwaiter().GetResult();
    }

    /// <summary>
    /// **Validates: Requirements 2.8**
    /// For any POST, PUT or DELETE to /api/feeds with a non-Admin User token,
    /// the server must return HTTP 403 Forbidden.
    /// The PublicAccessTestAuthHandler returns role "User" (not Admin) when a token is present.
    /// Tag: Feature: public-access, Property 7: write ops on /api/feeds require Admin
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "POST/PUT/DELETE /api/feeds com token não-Admin retornam 403")]
    public bool FeedsWriteOps_WithNonAdminToken_Return403(Guid id)
    {
        return Task.Run(async () =>
        {
            HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            // Add Authorization header — PublicAccessTestAuthHandler will authenticate as role "User" (not Admin)
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Test");

            HttpResponseMessage postResponse = await client.PostAsJsonAsync(
                "/api/feeds",
                new CreateFeedSourceRequest { Name = "Test", Url = $"https://test-{id}.example.com/feed" });

            if (postResponse.StatusCode != HttpStatusCode.Forbidden)
                throw new Exception(
                    $"Expected 403 Forbidden for POST /api/feeds but got {postResponse.StatusCode}");

            HttpResponseMessage putResponse = await client.PutAsJsonAsync(
                $"/api/feeds/{id}",
                new UpdateFeedSourceRequest { Name = "Updated" });

            if (putResponse.StatusCode != HttpStatusCode.Forbidden)
                throw new Exception(
                    $"Expected 403 Forbidden for PUT /api/feeds/{id} but got {putResponse.StatusCode}");

            HttpResponseMessage deleteResponse = await client.DeleteAsync(
                $"/api/feeds/{id}");

            if (deleteResponse.StatusCode != HttpStatusCode.Forbidden)
                throw new Exception(
                    $"Expected 403 Forbidden for DELETE /api/feeds/{id} but got {deleteResponse.StatusCode}");

            return true;
        }).GetAwaiter().GetResult();
    }
}
