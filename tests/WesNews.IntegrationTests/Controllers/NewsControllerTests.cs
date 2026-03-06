using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WesNews.Application.DTOs;
using WesNews.Infrastructure.Data;
using WesNews.IntegrationTests.Helpers;

namespace WesNews.IntegrationTests.Controllers;

public class NewsControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _client;

    public NewsControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await TestDbSeeder.SeedAsync(context);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetNews_ShouldReturn200_WithPagedResult()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/news");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<NewsArticleDto>? result = await response.Content.ReadFromJsonAsync<PagedResult<NewsArticleDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetNews_ShouldReturn200_FilteredByCategory()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/news?category=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<NewsArticleDto>? result = await response.Content.ReadFromJsonAsync<PagedResult<NewsArticleDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetNews_ShouldReturn200_FilteredBySearch()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/news?search=blazor");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedResult<NewsArticleDto>? result = await response.Content.ReadFromJsonAsync<PagedResult<NewsArticleDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturn204_WhenArticleExists()
    {
        HttpResponseMessage response = await _client.PatchAsync(
            $"/api/news/{TestDbSeeder.Article1Id}/read", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturn404_WhenArticleNotFound()
    {
        Guid unknownId = Guid.NewGuid();
        HttpResponseMessage response = await _client.PatchAsync($"/api/news/{unknownId}/read", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenArticleExists()
    {
        HttpResponseMessage response = await _client.DeleteAsync(
            $"/api/news/{TestDbSeeder.Article2Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
