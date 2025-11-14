using FluentValidation;

namespace Arda9UserApi.Application.Buckets.Commands.CreateBucket;

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

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("CompanyId é obrigatório");

        RuleFor(x => x.Region)
            .Must(region => string.IsNullOrEmpty(region) || IsValidAwsRegion(region))
            .WithMessage("Region inválida");
    }

    private bool IsValidAwsRegion(string region)
    {
        var validRegions = new[] { "us-east-1", "us-west-2", "eu-west-1", "sa-east-1" };
        return validRegions.Contains(region);
    }
}