using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Services;

namespace WesNews.UnitTests.Services;

public class FeedAggregatorServiceTests
{
    private readonly INewsArticleRepository _articleRepositoryMock;
    private readonly IHttpClientFactory _httpClientFactoryMock;
    private readonly ILogger<FeedAggregatorService> _loggerMock;
    private readonly FeedAggregatorService _sut;

    public FeedAggregatorServiceTests()
    {
        _articleRepositoryMock = Substitute.For<INewsArticleRepository>();
        _httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
        _loggerMock = Substitute.For<ILogger<FeedAggregatorService>>();
        
        _sut = new FeedAggregatorService(_articleRepositoryMock, _httpClientFactoryMock, _loggerMock);
    }

    [Fact]
    public async Task FetchAndSaveAsync_WithValidRssFeed_ShouldSaveArticles()
    {
        // Arrange
        FeedSource feedSource = new FeedSource { Id = Guid.NewGuid(), Name = "TestFeed", Url = "https://localhost/feed.xml" };
        string validRssXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <rss version=""2.0"">
              <channel>
                <title>Test Feed</title>
                <item>
                  <title>Test Article</title>
                  <link>https://localhost/article/1</link>
                  <description>Test Description</description>
                  <enclosure url=""https://localhost/image.jpg"" type=""image/jpeg"" />
                </item>
              </channel>
            </rss>";

        SetupHttpClientMock(validRssXml, HttpStatusCode.OK);

        List<NewsArticle> savedArticles = new List<NewsArticle>();
        _articleRepositoryMock
            .When(x => x.UpsertRangeAsync(Arg.Any<IEnumerable<NewsArticle>>(), Arg.Any<CancellationToken>()))
            .Do(callInfo => savedArticles.AddRange(callInfo.Arg<IEnumerable<NewsArticle>>()));

        // Act
        await _sut.FetchAndSaveAsync(feedSource, CancellationToken.None);

        // Assert
        savedArticles.Should().HaveCount(1);
        NewsArticle article = savedArticles.Single();
        article.Title.Should().Be("Test Article");
        article.Url.Should().Be("https://localhost/article/1");
        article.ImageUrl.Should().Be("https://localhost/image.jpg");
    }

    [Fact]
    public async Task FetchAndSaveAsync_WithInvalidXmlContent_ShouldNotSaveOrThrow()
    {
        // Arrange
        FeedSource feedSource = new FeedSource { Id = Guid.NewGuid(), Name = "TestFeed", Url = "https://localhost/feed.xml" };
        string invalidContent = "<html><body>Not an RSS feed</body></html>";
        
        SetupHttpClientMock(invalidContent, HttpStatusCode.OK);

        // Act
        Func<Task> act = async () => await _sut.FetchAndSaveAsync(feedSource, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _articleRepositoryMock.DidNotReceiveWithAnyArgs().UpsertRangeAsync(default!, default);
    }

    private void SetupHttpClientMock(string stringContent, HttpStatusCode statusCode)
    {
        HttpMessageHandler handlerMock = Substitute.ForPartsOf<MockHttpMessageHandler>(stringContent, statusCode);
        HttpClient client = new HttpClient(handlerMock)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _httpClientFactoryMock.CreateClient("FeedAggregator").Returns(client);
    }
}

public class MockHttpMessageHandler(string content, HttpStatusCode statusCode) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage responseMessage = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };
        
        return Task.FromResult(responseMessage);
    }
}
