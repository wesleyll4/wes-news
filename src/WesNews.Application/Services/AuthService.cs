using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Application.Services;

public class AuthService(IConfiguration configuration, IUserRepository userRepository) : IAuthService
{
    private readonly string _jwtKey = configuration["Jwt:Key"] ?? "DefaultSecretKeyForDevelopment12345!";
    private readonly string _jwtIssuer = configuration["Jwt:Issuer"] ?? "WesNews";
    private readonly string _jwtAudience = configuration["Jwt:Audience"] ?? "WesNewsUsers";
    private readonly int _jwtExpiryMinutes = configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        List<Claim> claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new LoginResponse
        {
            Token = tokenString,
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        bool exists = await userRepository.UsernameExistsAsync(request.Username, cancellationToken);
        if (exists)
        {
            return false;
        }

        User user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);

        return true;
    }
}