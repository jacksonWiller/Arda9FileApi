using Arda9File.Application.Application.Buckets.Commands.CreateBucket;
using FluentValidation;

namespace Arda9FileApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketValidator : AbstractValidator<CreateBucketCommand>
{
    public CreateBucketValidator()
    {
        RuleFor(x => x.BucketName)
            .NotEmpty().WithMessage("BucketName é obrigatório")
            .MinimumLength(3).WithMessage("BucketName deve ter no mínimo 3 caracteres")
            .MaximumLength(63).WithMessage("BucketName deve ter no máximo 63 caracteres")
            .Matches("^[a-z0-9][a-z0-9-]*[a-z0-9]$")
            .WithMessage("BucketName deve conter apenas letras minúsculas, números e hífens");


    }
}