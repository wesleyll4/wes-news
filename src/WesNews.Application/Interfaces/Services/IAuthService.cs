using WesNews.Application.DTOs;

namespace WesNews.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}