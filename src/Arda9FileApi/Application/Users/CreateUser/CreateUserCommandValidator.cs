using FluentValidation;

namespace Arda9FileApi.Application.Users.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .Must((command, phoneNumber) => 
                string.IsNullOrEmpty(phoneNumber) || 
                (!string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(command.PhoneCountryCode)))
            .WithMessage("PhoneCountryCode is required when PhoneNumber is provided");

        RuleFor(x => x.Roles)
            .Must(roles => roles == null || roles.Count <= 10)
            .WithMessage("User cannot have more than 10 roles");

        RuleFor(x => x.Locale)
            .Must(locale => string.IsNullOrEmpty(locale) || 
                new[] { "pt-BR", "en-US", "es-ES", "fr-FR" }.Contains(locale))
            .WithMessage("Locale must be one of: pt-BR, en-US, es-ES, fr-FR");

        RuleFor(x => x.PictureUrl)
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid URL format for PictureUrl");
    }
}