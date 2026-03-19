using System.ComponentModel.DataAnnotations;

namespace WesNews.Application.DTOs;

public class UpdateDigestPreferenceRequest
{
    [Required]
    public bool DigestEnabled { get; init; }
}

public class DigestPreferenceResponse
{
    public bool DigestEnabled { get; init; }
}

public class UpdateEmailRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;
}

public class UserProfileResponse
{
    public string Email { get; init; } = string.Empty;
    public bool DigestEnabled { get; init; }
}
