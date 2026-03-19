using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;
using WesNews.Domain.Entities;

namespace WesNews.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<DigestPreferenceResponse> GetDigestPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        User user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' not found.");

        return new DigestPreferenceResponse { DigestEnabled = user.DigestEnabled };
    }

    public async Task<DigestPreferenceResponse> UpdateDigestPreferenceAsync(Guid userId, bool digestEnabled, CancellationToken cancellationToken = default)
    {
        User user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' not found.");

        user.DigestEnabled = digestEnabled;
        await userRepository.UpdateAsync(user, cancellationToken);

        return new DigestPreferenceResponse { DigestEnabled = user.DigestEnabled };
    }

    public async Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _ = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' not found.");

        await userRepository.DeleteAsync(userId, cancellationToken);
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        User user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' not found.");

        return new UserProfileResponse { Email = user.Email, DigestEnabled = user.DigestEnabled };
    }

    public async Task<UserProfileResponse> UpdateEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        User user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' not found.");

        bool emailInUse = await userRepository.EmailExistsAsync(email, cancellationToken);
        if (emailInUse && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Email already in use.");

        user.Email = email;
        await userRepository.UpdateAsync(user, cancellationToken);

        return new UserProfileResponse { Email = user.Email, DigestEnabled = user.DigestEnabled };
    }
}
