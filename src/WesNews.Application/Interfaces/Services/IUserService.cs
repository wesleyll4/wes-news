using WesNews.Application.DTOs;

namespace WesNews.Application.Interfaces.Services;

public interface IUserService
{
    Task<DigestPreferenceResponse> GetDigestPreferenceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DigestPreferenceResponse> UpdateDigestPreferenceAsync(Guid userId, bool digestEnabled, CancellationToken cancellationToken = default);
    Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default);
}
