using FluentValidation;

namespace Arda9FileApi.Application.Buckets.Commands.DeleteBucket;

public class DeleteBucketValidator : AbstractValidator<DeleteBucketCommand>
{
    public DeleteBucketValidator()
    {
        RuleFor(x => x.BucketName)
            .NotEmpty().WithMessage("BucketName é obrigatório");
    }
}