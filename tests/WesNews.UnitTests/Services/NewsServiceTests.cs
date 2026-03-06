using FluentAssertions;
using NSubstitute;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Services;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;
using WesNews.UnitTests.Helpers;

namespace WesNews.UnitTests.Services;

public class NewsServiceTests
{
    private readonly INewsArticleRepository _repository;
    private readonly NewsService _sut;

    public NewsServiceTests()
    {
        _repository = Substitute.For<INewsArticleRepository>();
        _sut = new NewsService(_repository);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage_WhenCalled()
    {
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(3);
        PagedResult<NewsArticle> repoResult = new PagedResult<NewsArticle>(articles, 3, 1, 20);

        _repository.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(repoResult);

        PagedResult<NewsArticleDto> result = await _sut.GetPagedAsync(new NewsQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByCategory_WhenCategoryIsProvided()
    {
        NewsQuery query = new NewsQuery { Category = Category.DotNet };
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(2);
        PagedResult<NewsArticle> repoResult = new PagedResult<NewsArticle>(articles, 2, 1, 20);

        _repository.GetPagedAsync(
            Arg.Is<NewsQuery>(q => q.Category == Category.DotNet),
            Arg.Any<CancellationToken>())
            .Returns(repoResult);

        PagedResult<NewsArticleDto> result = await _sut.GetPagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        await _repository.Received(1).GetPagedAsync(
            Arg.Is<NewsQuery>(q => q.Category == Category.DotNet),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterBySearchTerm_WhenSearchIsProvided()
    {
        NewsQuery query = new NewsQuery { Search = "blazor" };
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(1);
        PagedResult<NewsArticle> repoResult = new PagedResult<NewsArticle>(articles, 1, 1, 20);

        _repository.GetPagedAsync(
            Arg.Is<NewsQuery>(q => q.Search == "blazor"),
            Arg.Any<CancellationToken>())
            .Returns(repoResult);

        PagedResult<NewsArticleDto> result = await _sut.GetPagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnOnlyUnread_WhenUnreadOnlyIsTrue()
    {
        NewsQuery query = new NewsQuery { UnreadOnly = true };
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(2, isRead: false);
        PagedResult<NewsArticle> repoResult = new PagedResult<NewsArticle>(articles, 2, 1, 20);

        _repository.GetPagedAsync(
            Arg.Is<NewsQuery>(q => q.UnreadOnly),
            Arg.Any<CancellationToken>())
            .Returns(repoResult);

        PagedResult<NewsArticleDto> result = await _sut.GetPagedAsync(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(a => a.IsRead.Should().BeFalse());
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldReturnTrue_WhenArticleExists()
    {
        Guid articleId = Guid.NewGuid();
        _repository.MarkAsReadAsync(articleId, Arg.Any<CancellationToken>()).Returns(true);

        bool result = await _sut.MarkAsReadAsync(articleId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldReturnFalse_WhenArticleNotFound()
    {
        Guid articleId = Guid.NewGuid();
        _repository.MarkAsReadAsync(articleId, Arg.Any<CancellationToken>()).Returns(false);

        bool result = await _sut.MarkAsReadAsync(articleId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository_WhenArticleExists()
    {
        Guid articleId = Guid.NewGuid();
        _repository.DeleteAsync(articleId, Arg.Any<CancellationToken>()).Returns(true);

        bool result = await _sut.DeleteAsync(articleId, CancellationToken.None);

        result.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(articleId, Arg.Any<CancellationToken>());
    }
}
