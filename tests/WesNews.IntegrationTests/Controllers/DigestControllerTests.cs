using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WesNews.Application.DTOs;
using WesNews.Infrastructure.Data;
using WesNews.IntegrationTests.Helpers;

namespace WesNews.IntegrationTests.Controllers;

public class DigestControllerTests(CustomWebAppFactory factory) : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
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
    public async Task Preview_ShouldReturn200_WithHtml()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/digest/preview");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        DigestPreviewDto? preview = await response.Content.ReadFromJsonAsync<DigestPreviewDto>();
        preview.Should().NotBeNull();
        preview!.Html.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Send_ShouldReturn202_Accepted()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/digest/send", null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
}
