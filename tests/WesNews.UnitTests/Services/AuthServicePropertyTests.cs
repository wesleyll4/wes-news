using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Services;
using WesNews.Domain.Entities;

namespace WesNews.UnitTests.Services;

/// <summary>
/// Feature: user-digest-preference, Property 1: Novo usuário tem DigestEnabled false
/// Feature: user-digest-preference, Property 3: Login retorna DigestEnabled correto
/// Validates: Requirements 1.1, 1.2, 1.4
/// </summary>
public class AuthServicePropertyTests
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly AuthService _sut;

    public AuthServicePropertyTests()
    {
        _userRepository = Substitute.For<IUserRepository>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretKeyForTestingPurposesOnly123!",
                ["Jwt:Issuer"] = "WesNews",
                ["Jwt:Audience"] = "WesNewsUsers",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        _sut = new AuthService(_configuration, _userRepository);
    }

    /// <summary>
    /// **Validates: Requirements 1.4**
    /// For any user with DigestEnabled set to true or false, LoginAsync must return
    /// a LoginResponse with DigestEnabled equal to the stored value.
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Login retorna DigestEnabled correto")]
    public bool LoginAsync_ReturnsCorrectDigestEnabled(bool digestEnabled)
    {
        string username = $"user_{Guid.NewGuid():N}";
        string password = "TestPassword123!";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        User user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@example.com",
            PasswordHash = passwordHash,
            FullName = "Test User",
            Role = "User",
            DigestEnabled = digestEnabled,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.GetByUsernameAsync(username, Arg.Any<CancellationToken>())
            .Returns(user);

        LoginRequest request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        LoginResponse? response = _sut.LoginAsync(request).GetAwaiter().GetResult();

        return response != null && response.DigestEnabled == digestEnabled;
    }

    /// <summary>
    /// **Validates: Requirements 1.1, 1.2**
    /// For any valid registration data, after RegisterAsync the user stored in the
    /// repository must have DigestEnabled == false.
    /// Tag: Feature: user-digest-preference, Property 1: Novo usuário tem DigestEnabled false
    /// </summary>
    [Property(MaxTest = 100, DisplayName = "Novo usuário tem DigestEnabled false")]
    public Property RegisterAsync_NewUserHasDigestEnabledFalse()
    {
        string[] names = ["alice", "bob", "carol", "dave", "eve", "frank", "grace", "heidi"];
        string[] prefixes = ["test", "user", "admin", "info"];
        string[] passwords = ["password123", "securePass!", "mySecret99", "P@ssw0rd!"];
        string[] fullNames = ["Alice Smith", "Bob Jones", "Carol White", "Dave Brown"];

        Arbitrary<(string username, string email, string password, string fullName)> validRegistrationData =
            Arb.From(
                from name in Gen.Elements(names)
                from suffix in Gen.Choose(1000, 9999)
                from prefix in Gen.Elements(prefixes)
                from emailSuffix in Gen.Choose(1000, 9999)
                from password in Gen.Elements(passwords)
                from fullName in Gen.Elements(fullNames)
                let username = $"{name}{suffix}"
                let email = $"{prefix}{emailSuffix}@example.com"
                select (username, email, password, fullName)
            );

        return Prop.ForAll(validRegistrationData, data =>
        {
            IUserRepository repo = Substitute.For<IUserRepository>();
            AuthService sut = new AuthService(_configuration, repo);

            repo.UsernameExistsAsync(data.username, Arg.Any<CancellationToken>())
                .Returns(false);

            User? capturedUser = null;
            repo.AddAsync(Arg.Do<User>(u => capturedUser = u), Arg.Any<CancellationToken>())
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            bool result = sut.RegisterAsync(new RegisterRequest
            {
                Username = data.username,
                Email = data.email,
                Password = data.password,
                FullName = data.fullName
            }).GetAwaiter().GetResult();

            return result && capturedUser != null && capturedUser.DigestEnabled == false;
        });
    }
}
