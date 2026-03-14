using FluentValidation;
using WesNews.Application.DTOs;

namespace WesNews.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required")
            .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters");
    }
}
