using FluentValidation;
using CondoSync.Application.Features.Bookings.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.BookingDate).NotEmpty()
            .Must(d => d >= DateTime.UtcNow.Date).WithMessage("Data da reserva deve ser hoje ou futura");
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.GuestsCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SpecialRequirements).MaximumLength(2000);
    }
}

public class ApproveRejectRequestValidator : AbstractValidator<ApproveRejectRequest>
{
    public ApproveRejectRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
