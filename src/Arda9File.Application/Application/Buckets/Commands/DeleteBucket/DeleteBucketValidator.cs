using FluentValidation;

namespace Arda9File.Application.Application.Buckets.Commands.DeleteBucket;

public class DeleteBucketValidator : AbstractValidator<DeleteBucketCommand>
{
    public DeleteBucketValidator()
    {
        RuleFor(x => x.BucketName)
            .NotEmpty().WithMessage("BucketName é obrigatório");
    }
}