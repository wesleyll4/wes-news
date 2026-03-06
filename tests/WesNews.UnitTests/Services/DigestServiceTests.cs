using FluentAssertions;
using NSubstitute;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Application.Services;
using WesNews.Domain.Entities;
using WesNews.UnitTests.Helpers;

namespace WesNews.UnitTests.Services;

public class DigestServiceTests
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly IDigestEmailService _emailService;
    private readonly DigestService _sut;

    public DigestServiceTests()
    {
        _articleRepository = Substitute.For<INewsArticleRepository>();
        _emailService = Substitute.For<IDigestEmailService>();
        _sut = new DigestService(_articleRepository, _emailService);
    }

    [Fact]
    public async Task BuildDigestArticlesAsync_ShouldReturnArticles_WhenUnreadExist()
    {
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(3);
        PagedResult<NewsArticle> pagedResult = new PagedResult<NewsArticle>(articles, 3, 1, 5);

        _articleRepository.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        IReadOnlyList<NewsArticle> digest = await _sut.BuildDigestArticlesAsync(CancellationToken.None);

        digest.Should().NotBeEmpty();
    }

    [Fact]
    public async Task BuildDigestArticlesAsync_ShouldReturnEmpty_WhenNoUnreadArticles()
    {
        PagedResult<NewsArticle> emptyResult = new PagedResult<NewsArticle>(
            new List<NewsArticle>().AsReadOnly(), 0, 1, 5);

        _articleRepository.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(emptyResult);

        IReadOnlyList<NewsArticle> digest = await _sut.BuildDigestArticlesAsync(CancellationToken.None);

        digest.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_ShouldCallEmailService_WithCorrectArticles()
    {
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(2);
        PagedResult<NewsArticle> pagedResult = new PagedResult<NewsArticle>(articles, 2, 1, 5);

        _articleRepository.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        await _sut.SendAsync(CancellationToken.None);

        await _emailService.Received(1).SendAsync(
            Arg.Any<IEnumerable<NewsArticle>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPreviewAsync_ShouldReturnHtmlString_WhenArticlesExist()
    {
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(2);
        PagedResult<NewsArticle> pagedResult = new PagedResult<NewsArticle>(articles, 2, 1, 5);

        _articleRepository.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        _emailService.BuildPreviewHtml(Arg.Any<IEnumerable<NewsArticle>>())
            .Returns("<html><body>Daily Digest Preview</body></html>");

        DigestPreviewDto preview = await _sut.GetPreviewAsync(CancellationToken.None);

        preview.Html.Should().NotBeEmpty();
        preview.ArticleCount.Should().BeGreaterThan(0);
    }
}
