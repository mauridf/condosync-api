using FluentValidation;
using CondoSync.Application.Features.Services.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug deve conter apenas letras minúsculas, números e hífens");
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ServiceType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceUnit).MaximumLength(20);
        RuleFor(x => x.MaxBookingPerDay).GreaterThan(0).When(x => x.MaxBookingPerDay.HasValue);
        RuleFor(x => x.MaxBookingPerUser).GreaterThan(0).When(x => x.MaxBookingPerUser.HasValue);
        RuleFor(x => x.AdvanceBookingDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CancelBeforeHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SlotDuration).GreaterThan(0).When(x => x.SlotDuration.HasValue);
    }
}

public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}
