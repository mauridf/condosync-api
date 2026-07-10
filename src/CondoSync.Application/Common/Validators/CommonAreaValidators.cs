using FluentValidation;
using CondoSync.Application.Features.CommonAreas.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateCommonAreaRequestValidator : AbstractValidator<CreateCommonAreaRequest>
{
    public CreateCommonAreaRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}

public class UpdateCommonAreaRequestValidator : AbstractValidator<UpdateCommonAreaRequest>
{
    public UpdateCommonAreaRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}
