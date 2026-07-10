using FluentValidation;
using CondoSync.Application.Features.Units.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateUnitRequestValidator : AbstractValidator<CreateUnitRequest>
{
    public CreateUnitRequestValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Block).MaximumLength(50);
        RuleFor(x => x.Floor).MaximumLength(20);
        RuleFor(x => x.Area).GreaterThan(0).When(x => x.Area.HasValue);
        RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bathrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ParkingSpots).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlyFee).GreaterThan(0).When(x => x.MonthlyFee.HasValue);
    }
}

public class UpdateUnitRequestValidator : AbstractValidator<UpdateUnitRequest>
{
    public UpdateUnitRequestValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Area).GreaterThan(0).When(x => x.Area.HasValue);
        RuleFor(x => x.MonthlyFee).GreaterThan(0).When(x => x.MonthlyFee.HasValue);
    }
}

public class BatchCreateUnitsRequestValidator : AbstractValidator<BatchCreateUnitsRequest>
{
    public BatchCreateUnitsRequestValidator()
    {
        RuleFor(x => x.Units).NotEmpty().WithMessage("Lista de unidades não pode estar vazia");
        RuleForEach(x => x.Units).SetValidator(new CreateUnitRequestValidator());
    }
}
