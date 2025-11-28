using FluentValidation;

namespace Arda9FileApi.Application.Features.Files.Commands.UpdateFile;

public class UpdateFileCommandValidator : AbstractValidator<UpdateFileCommand>
{
    public UpdateFileCommandValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithMessage("FileId is required");

        RuleFor(x => x.FileName)
            .MaximumLength(255)
            .WithMessage("FileName must not exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.FileName));
    }
}