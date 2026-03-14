using FluentValidation;

namespace Arda9File.Application.Application.Folders.Commands.CreateFolder;

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(x => x.FolderName)
            .NotEmpty()
            .WithMessage("FolderName is required")
            .MaximumLength(255)
            .WithMessage("FolderName must not exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9-_\s]+$")
            .WithMessage("FolderName can only contain letters, numbers, hyphens, underscores and spaces");

        RuleFor(x => x.BucketId)
            .NotEmpty()
            .WithMessage("BucketId is required");

    }
}