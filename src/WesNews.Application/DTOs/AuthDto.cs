using System.ComponentModel.DataAnnotations;

namespace WesNews.Application.DTOs;

public class LoginRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Username { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; init; } = string.Empty;
}

public class RegisterRequest
{
    [Required(AllowEmptyStrings = false)]
    [MinLength(3)]
    public string Username { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string FullName { get; init; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public bool DigestEnabled { get; init; }
}