using WesNews.Application.DTOs;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Application.Interfaces.Services;

namespace WesNews.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<DigestPreferenceResponse> GetDigestPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' not found.");

        return new DigestPreferenceResponse { DigestEnabled = user.DigestEnabled };
    }

    public async Task<DigestPreferenceResponse> UpdateDigestPreferenceAsync(Guid userId, bool digestEnabled, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
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
}
