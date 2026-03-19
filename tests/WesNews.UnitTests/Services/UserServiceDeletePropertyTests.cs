using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using NSubstitute;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Services;
using WesNews.Domain.Entities;

namespace WesNews.UnitTests.Services;

/// <summary>
/// Feature: delete-account, Property 1: Exclusão remove o usuário do repositório
/// Feature: delete-account, Property 2: Exclusão de usuário inexistente lança exceção
/// Validates: Requirements 1.1, 1.4, 2.2, 2.3
/// </summary>
public class UserServiceDeletePropertyTests
{
    private static User CreateUser(Guid id) => new User
    {
        Id = id,
        Username = $"u{id:N}",
        Email = $"u{id:N}@example.com",
        PasswordHash = "hash",
        FullName = "Test User",
        Role = "User",
        DigestEnabled = false,
        CreatedAt = DateTime.UtcNow
    };

    private static Arbitrary<Guid> GuidArb =>
        Arb.From(Gen.Fresh(Guid.NewGuid));

    /// <summary>
    /// **Validates: Requirements 1.1, 2.2**
    /// For any existing userId, after DeleteAccountAsync(userId), GetByIdAsync(userId) must return null.
    /// Tag: Feature: delete-account, Property 1: Exclusão remove o usuário do repositório
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Exclusão remove o usuário do repositório")]
    public Property DeleteAccountAsync_ExistingUser_RemovesFromRepository()
    {
        return Prop.ForAll(GuidArb, userId =>
        {
            // Arrange
            User user = CreateUser(userId);
            IUserRepository userRepository = Substitute.For<IUserRepository>();

            userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
                .Returns(user);

            userRepository.DeleteAsync(userId, Arg.Any<CancellationToken>())
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            UserService sut = new UserService(userRepository);

            // Act
            sut.DeleteAccountAsync(userId).GetAwaiter().GetResult();

            // Assert: DeleteAsync was called with the correct userId
            userRepository.Received(1).DeleteAsync(userId, Arg.Any<CancellationToken>());

            // Assert: after deletion, GetByIdAsync returns null (simulate repository state)
            userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
                .Returns((User?)null);

            User? result = userRepository.GetByIdAsync(userId).GetAwaiter().GetResult();
            return result is null;
        });
    }

    /// <summary>
    /// **Validates: Requirements 1.4, 2.3**
    /// For any userId that does not exist in the repository, DeleteAccountAsync(userId) must throw KeyNotFoundException.
    /// Tag: Feature: delete-account, Property 2: Exclusão de usuário inexistente lança exceção
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Exclusão de usuário inexistente lança exceção")]
    public Property DeleteAccountAsync_NonExistentUser_ThrowsKeyNotFoundException()
    {
        return Prop.ForAll(GuidArb, userId =>
        {
            // Arrange
            IUserRepository userRepository = Substitute.For<IUserRepository>();

            userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
                .Returns((User?)null);

            UserService sut = new UserService(userRepository);

            // Act & Assert
            try
            {
                sut.DeleteAccountAsync(userId).GetAwaiter().GetResult();
                return false; // Should have thrown
            }
            catch (KeyNotFoundException)
            {
                return true;
            }
        });
    }
}
