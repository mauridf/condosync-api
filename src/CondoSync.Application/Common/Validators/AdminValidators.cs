using FluentValidation;
using CondoSync.Application.Features.Admin.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateCondominiumRequestValidator : AbstractValidator<CreateCondominiumRequest>
{
    public CreateCondominiumRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens");
        RuleFor(x => x.AdminName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.Cnpj).MaximumLength(18);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Plan).MaximumLength(50);
    }
}

public class UpdateCondominiumRequestValidator : AbstractValidator<UpdateCondominiumRequest>
{
    public UpdateCondominiumRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(2);
        RuleFor(x => x.ZipCode).MaximumLength(9);
    }
}

public class ChangePlanRequestValidator : AbstractValidator<ChangePlanRequest>
{
    public ChangePlanRequestValidator()
    {
        RuleFor(x => x.Plan).NotEmpty().MaximumLength(50)
            .Must(p => new[] { "trial", "free", "basic", "premium", "enterprise" }.Contains(p))
            .WithMessage("Plano deve ser: trial, free, basic, premium ou enterprise");
    }
}
