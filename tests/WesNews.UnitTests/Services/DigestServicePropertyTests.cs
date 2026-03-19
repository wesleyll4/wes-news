using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Application.Services;
using WesNews.Domain.Entities;
using WesNews.UnitTests.Helpers;

namespace WesNews.UnitTests.Services;

/// <summary>
/// Feature: user-digest-preference, Property 6: DigestService envia apenas para usuários elegíveis
/// Feature: user-digest-preference, Property 7: Cardinalidade de envios igual ao número de destinatários
/// Feature: user-digest-preference, Property 8: Falha em um destinatário não interrompe os demais
/// Validates: Requirements 3.1, 3.3, 3.4
/// </summary>
public class DigestServicePropertyTests
{
    private static DigestService CreateSut(
        IUserRepository userRepository,
        IDigestEmailService emailService,
        INewsArticleRepository? articleRepository = null,
        ILogger<DigestService>? logger = null)
    {
        articleRepository ??= CreateDefaultArticleRepository();
        logger ??= Substitute.For<ILogger<DigestService>>();
        return new DigestService(articleRepository, emailService, userRepository, logger);
    }

    private static INewsArticleRepository CreateDefaultArticleRepository()
    {
        INewsArticleRepository repo = Substitute.For<INewsArticleRepository>();
        IReadOnlyList<NewsArticle> articles = FakeData.CreateNewsArticles(2);
        PagedResult<NewsArticle> pagedResult = new PagedResult<NewsArticle>(articles, 2, 1, 5);
        repo.GetPagedAsync(Arg.Any<NewsQuery>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);
        return repo;
    }

    private static User CreateUser() => new User
    {
        Id = Guid.NewGuid(),
        Username = $"user_{Guid.NewGuid():N}",
        Email = $"user_{Guid.NewGuid():N}@example.com",
        PasswordHash = "hash",
        FullName = "Test User",
        Role = "User",
        DigestEnabled = true,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// **Validates: Requirements 3.1**
    /// For any set of users with a mix of DigestEnabled values, SendToRecipientAsync
    /// must be called exactly once per user returned by GetDigestEnabledUsersAsync
    /// and never for users not returned by it.
    /// Tag: Feature: user-digest-preference, Property 6: DigestService envia apenas para usuários elegíveis
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "DigestService envia apenas para usuários elegíveis")]
    public Property SendAsync_CallsSendToRecipientOnlyForEligibleUsers()
    {
        Arbitrary<List<User>> usersGen = Arb.From(
            from count in Gen.Choose(0, 10)
            from users in Gen.ListOf<User>(Gen.Fresh(CreateUser), count)
            select users.ToList()
        );

        return Prop.ForAll(usersGen, eligibleUsers =>
        {
            IUserRepository userRepository = Substitute.For<IUserRepository>();
            IDigestEmailService emailService = Substitute.For<IDigestEmailService>();

            userRepository.GetDigestEnabledUsersAsync(Arg.Any<CancellationToken>())
                .Returns(eligibleUsers);

            emailService.SendToRecipientAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>())
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            DigestService sut = CreateSut(userRepository, emailService);
            sut.SendAsync(CancellationToken.None).GetAwaiter().GetResult();

            emailService.Received(eligibleUsers.Count).SendToRecipientAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>());

            foreach (User user in eligibleUsers)
            {
                emailService.Received(1).SendToRecipientAsync(
                    user.Email,
                    Arg.Any<IEnumerable<NewsArticle>>(),
                    Arg.Any<CancellationToken>());
            }

            return true;
        });
    }

    /// <summary>
    /// **Validates: Requirements 3.3**
    /// For any non-empty list of eligible users, the number of calls to SendToRecipientAsync
    /// must equal the size of the list.
    /// Tag: Feature: user-digest-preference, Property 7: Cardinalidade de envios igual ao número de destinatários
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Cardinalidade de envios igual ao número de destinatários")]
    public Property SendAsync_CallCountEqualToRecipientListSize()
    {
        Arbitrary<List<User>> nonEmptyUsersGen = Arb.From(
            from count in Gen.Choose(1, 10)
            from users in Gen.ListOf<User>(Gen.Fresh(CreateUser), count)
            select users.ToList()
        );

        return Prop.ForAll(nonEmptyUsersGen, eligibleUsers =>
        {
            IUserRepository userRepository = Substitute.For<IUserRepository>();
            IDigestEmailService emailService = Substitute.For<IDigestEmailService>();

            userRepository.GetDigestEnabledUsersAsync(Arg.Any<CancellationToken>())
                .Returns(eligibleUsers);

            emailService.SendToRecipientAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>())
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            DigestService sut = CreateSut(userRepository, emailService);
            sut.SendAsync(CancellationToken.None).GetAwaiter().GetResult();

            emailService.Received(eligibleUsers.Count).SendToRecipientAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>());

            return true;
        });
    }

    /// <summary>
    /// **Validates: Requirements 3.4**
    /// For any list of 2+ recipients where sending to one throws an exception,
    /// the others should still receive the digest and the error should be logged.
    /// Tag: Feature: user-digest-preference, Property 8: Falha em um destinatário não interrompe os demais
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Falha em um destinatário não interrompe os demais")]
    public Property SendAsync_FailureForOneRecipientDoesNotInterruptOthers()
    {
        // Generate a list of 2+ users and an index indicating which one will fail
        Arbitrary<(List<User> Users, int FailingIndex)> gen = Arb.From(
            from count in Gen.Choose(2, 8)
            from users in Gen.ListOf<User>(Gen.Fresh(CreateUser), count)
            from failingIndex in Gen.Choose(0, count - 1)
            select (users.ToList(), failingIndex)
        );

        return Prop.ForAll(gen, input =>
        {
            List<User> users = input.Users;
            int failingIndex = input.FailingIndex;
            string failingEmail = users[failingIndex].Email;

            IUserRepository userRepository = Substitute.For<IUserRepository>();
            IDigestEmailService emailService = Substitute.For<IDigestEmailService>();
            ILogger<DigestService> logger = Substitute.For<ILogger<DigestService>>();

            userRepository.GetDigestEnabledUsersAsync(Arg.Any<CancellationToken>())
                .Returns(users);

            // Make the failing recipient throw, all others succeed
            emailService.SendToRecipientAsync(
                Arg.Is<string>(e => e != failingEmail),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>())
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            emailService.SendToRecipientAsync(
                Arg.Is<string>(e => e == failingEmail),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>())
                .Returns<System.Threading.Tasks.Task>(_ => throw new InvalidOperationException($"Send failed for {failingEmail}"));

            DigestService sut = CreateSut(userRepository, emailService, logger: logger);

            // Should not throw — failure is swallowed per-recipient
            sut.SendAsync(CancellationToken.None).GetAwaiter().GetResult();

            // All recipients (including the failing one) must have been attempted
            emailService.Received(users.Count).SendToRecipientAsync(
                Arg.Any<string>(),
                Arg.Any<IEnumerable<NewsArticle>>(),
                Arg.Any<CancellationToken>());

            // Every non-failing recipient must have been called exactly once
            foreach (User user in users.Where(u => u.Email != failingEmail))
            {
                emailService.Received(1).SendToRecipientAsync(
                    user.Email,
                    Arg.Any<IEnumerable<NewsArticle>>(),
                    Arg.Any<CancellationToken>());
            }

            // The error must have been logged (LogError called at least once)
            logger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());

            return true;
        });
    }
}
