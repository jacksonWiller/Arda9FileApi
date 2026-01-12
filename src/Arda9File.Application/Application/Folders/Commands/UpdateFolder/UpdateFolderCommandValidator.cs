using FluentValidation;

namespace Arda9File.Application.Application.Folders.Commands.UpdateFolder;

public class UpdateFolderCommandValidator : AbstractValidator<UpdateFolderCommand>
{
    public UpdateFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty()
            .WithMessage("FolderId is required");

        RuleFor(x => x.FolderName)
            .MaximumLength(255)
            .WithMessage("FolderName must not exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9-_\s]+$")
            .WithMessage("FolderName can only contain letters, numbers, hyphens, underscores and spaces")
            .When(x => !string.IsNullOrEmpty(x.FolderName));
    }
}