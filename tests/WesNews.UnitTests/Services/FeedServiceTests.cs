using FluentAssertions;
using NSubstitute;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Services;
using WesNews.Domain.Entities;
using WesNews.Domain.Enums;
using WesNews.UnitTests.Helpers;

namespace WesNews.UnitTests.Services;

public class FeedServiceTests
{
    private readonly IFeedSourceRepository _repository;
    private readonly FeedService _sut;

    public FeedServiceTests()
    {
        _repository = Substitute.For<IFeedSourceRepository>();
        _sut = new FeedService(_repository);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllFeeds()
    {
        IReadOnlyList<FeedSource> feeds = FakeData.CreateFeedSources(3);
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(feeds);

        IReadOnlyList<FeedSourceDto> result = await _sut.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnCreatedFeed_WhenDataIsValid()
    {
        CreateFeedSourceRequest request = new CreateFeedSourceRequest
        {
            Name = "New Feed",
            Url = "https://new-feed.com/rss",
            Category = Category.DotNet
        };

        _repository.ExistsByUrlAsync(request.Url, Arg.Any<CancellationToken>()).Returns(false);
        _repository.AddAsync(Arg.Any<FeedSource>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<FeedSource>());

        FeedSourceDto result = await _sut.AddAsync(request, CancellationToken.None);

        result.Name.Should().Be("New Feed");
        result.Url.Should().Be(request.Url);
        result.Category.Should().Be(Category.DotNet);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException_WhenUrlAlreadyExists()
    {
        CreateFeedSourceRequest request = new CreateFeedSourceRequest
        {
            Name = "Duplicate",
            Url = "https://existing.com/rss",
            Category = Category.General
        };

        _repository.ExistsByUrlAsync(request.Url, Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = async () => await _sut.AddAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepository_WhenFeedExists()
    {
        Guid feedId = Guid.NewGuid();
        _repository.DeleteAsync(feedId, Arg.Any<CancellationToken>()).Returns(true);

        bool result = await _sut.DeleteAsync(feedId, CancellationToken.None);

        result.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(feedId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ToggleActiveAsync_ShouldInvertActiveState()
    {
        Guid feedId = Guid.NewGuid();
        FeedSource feed = FakeData.CreateFeedSource(id: feedId, isActive: true);

        _repository.GetByIdAsync(feedId, Arg.Any<CancellationToken>()).Returns(feed);
        _repository.UpdateAsync(Arg.Any<FeedSource>(), Arg.Any<CancellationToken>()).Returns(true);

        UpdateFeedSourceRequest request = new UpdateFeedSourceRequest { IsActive = false };
        bool result = await _sut.UpdateAsync(feedId, request, CancellationToken.None);

        result.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(
            Arg.Is<FeedSource>(f => !f.IsActive),
            Arg.Any<CancellationToken>());
    }
}
