using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WesNews.Application.DTOs;
using WesNews.Domain.Enums;
using WesNews.Infrastructure.Data;
using WesNews.IntegrationTests.Helpers;

namespace WesNews.IntegrationTests.Controllers;

public class FeedsControllerTests(CustomWebAppFactory factory) : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await TestDbSeeder.SeedAsync(context);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetFeeds_ShouldReturn200()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/feeds");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        IReadOnlyList<FeedSourceDto>? feeds = await response.Content.ReadFromJsonAsync<IReadOnlyList<FeedSourceDto>>();
        feeds.Should().NotBeNull();
        feeds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddFeed_ShouldReturn201_WhenDataIsValid()
    {
        CreateFeedSourceRequest request = new CreateFeedSourceRequest
        {
            Name = "New Test Feed",
            Url = "https://new-test.com/feed",
            Category = Category.AI
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/feeds", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddFeed_ShouldReturn409_WhenUrlAlreadyExists()
    {
        CreateFeedSourceRequest request = new CreateFeedSourceRequest
        {
            Name = "Duplicate",
            Url = "https://test-feed.com/rss",
            Category = Category.DotNet
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/feeds", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddFeed_ShouldReturn400_WhenBodyIsInvalid()
    {
        CreateFeedSourceRequest request = new CreateFeedSourceRequest
        {
            Name = "",
            Url = "",
            Category = Category.General
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/feeds", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateFeed_ShouldReturn204_WhenFeedExists()
    {
        UpdateFeedSourceRequest request = new UpdateFeedSourceRequest { IsActive = false };

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/feeds/{TestDbSeeder.FeedId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteFeed_ShouldReturn204_WhenFeedExists()
    {
        HttpResponseMessage response = await _client.DeleteAsync(
            $"/api/feeds/{TestDbSeeder.FeedId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
