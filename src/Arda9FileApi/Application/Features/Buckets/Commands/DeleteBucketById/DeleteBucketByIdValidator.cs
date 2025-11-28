using FluentValidation;

namespace Arda9FileApi.Application.Features.Buckets.Commands.DeleteBucketById;

public class DeleteBucketByIdValidator : AbstractValidator<DeleteBucketByIdCommand>
{
    public DeleteBucketByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("O ID do bucket é obrigatório");
    }
}