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
