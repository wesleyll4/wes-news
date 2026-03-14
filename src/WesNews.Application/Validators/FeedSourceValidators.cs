using FluentValidation;
using WesNews.Application.DTOs;

namespace WesNews.Application.Validators;

public class CreateFeedSourceRequestValidator : AbstractValidator<CreateFeedSourceRequest>
{
    public CreateFeedSourceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out Uri? outUri) && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must be a valid HTTP or HTTPS absolute URI.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid category.");
    }
}

public class UpdateFeedSourceRequestValidator : AbstractValidator<UpdateFeedSourceRequest>
{
    public UpdateFeedSourceRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Name must be at least 3 characters long.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid category.")
            .When(x => x.Category.HasValue);
    }
}
