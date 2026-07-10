using CondoSync.Application.Features.Notifications.DTOs;
using CondoSync.Application.Features.Auth.DTOs;
using FluentValidation;

namespace CondoSync.Application.Common.Validators;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class Enable2FaRequestValidator : AbstractValidator<Enable2FaRequest>
{
    public Enable2FaRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}

public class Verify2FaRequestValidator : AbstractValidator<Verify2FaRequest>
{
    public Verify2FaRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone != null);
    }
}
