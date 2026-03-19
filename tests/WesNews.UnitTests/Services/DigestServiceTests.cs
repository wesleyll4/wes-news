using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DigestService> logger;
    private readonly DigestService _sut;


    public DigestServiceTests()
    {
        _articleRepository = Substitute.For<INewsArticleRepository>();
        _emailService = Substitute.For<IDigestEmailService>();
        _userRepository = Substitute.For<IUserRepository>();
        logger = Substitute.For<ILogger<DigestService>>();
        _sut = new DigestService(_articleRepository, _emailService, _userRepository, logger);
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
    public async Task SendAsync_ShouldCallSendToRecipientAsync_ForEachEligibleUser()
    {
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(2);
        PagedResult<NewsArticle> pagedResult = new PagedResult<NewsArticle>(articles, 2, 1, 5);

        _articleRepository.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        List<User> users =
        [
            new User { Email = "user1@example.com", DigestEnabled = true },
            new User { Email = "user2@example.com", DigestEnabled = true }
        ];
        _userRepository.GetDigestEnabledUsersAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        await _sut.SendAsync(CancellationToken.None);

        await _emailService.Received(2).SendToRecipientAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<NewsArticle>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_ShouldNotSendEmails_AndLogInformative_WhenNoEligibleUsers()
    {
        _userRepository.GetDigestEnabledUsersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        await _sut.SendAsync(CancellationToken.None);

        await _emailService.DidNotReceive().SendToRecipientAsync(
            Arg.Any<string>(),
            Arg.Any<IEnumerable<NewsArticle>>(),
            Arg.Any<CancellationToken>());

        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("No users") || o.ToString()!.Contains("Skipping")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendAsync_ShouldNotSendEmails_WhenNoEligibleUsers()
    {
        _userRepository.GetDigestEnabledUsersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        await _sut.SendAsync(CancellationToken.None);

        await _emailService.DidNotReceive().SendToRecipientAsync(
            Arg.Any<string>(),
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
