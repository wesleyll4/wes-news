using FsCheck;
using FsCheck.Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WesNews.Domain.Entities;
using WesNews.Infrastructure.Data;
using WesNews.Infrastructure.Repositories;

namespace WesNews.UnitTests.Repositories;

/// <summary>
/// Feature: user-digest-preference, Property 2: Round-trip de persistência da preferência
/// Validates: Requirements 1.3
/// </summary>
public class UserRepositoryPropertyTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public UserRepositoryPropertyTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using AppDbContext ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    private AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// **Validates: Requirements 1.3**
    /// For any boolean value v, updating DigestEnabled to v via repository and then
    /// fetching the user should return the same value v (round-trip persistence).
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Round-trip de persistência da preferência")]
    public bool DigestEnabled_RoundTrip_PersistsCorrectly(bool v)
    {
        using AppDbContext context = CreateContext();
        UserRepository repository = new UserRepository(context);

        User user = new User
        {
            Id = Guid.NewGuid(),
            Username = $"user_{Guid.NewGuid():N}",
            Email = $"user_{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            FullName = "Test User",
            DigestEnabled = !v
        };

        repository.AddAsync(user).GetAwaiter().GetResult();

        user.DigestEnabled = v;
        repository.UpdateAsync(user).GetAwaiter().GetResult();

        User? fetched = repository.GetByIdAsync(user.Id).GetAwaiter().GetResult();

        return fetched != null && fetched.DigestEnabled == v;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
