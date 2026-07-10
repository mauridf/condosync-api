using FluentValidation;
using CondoSync.Application.Features.Tickets.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Priority).MaximumLength(20);
        RuleFor(x => x.Subcategory).MaximumLength(100);
    }
}

public class UpdateTicketStatusRequestValidator : AbstractValidator<UpdateTicketStatusRequest>
{
    public UpdateTicketStatusRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Resolution).MaximumLength(4000);
    }
}

public class AddTicketMessageRequestValidator : AbstractValidator<AddTicketMessageRequest>
{
    public AddTicketMessageRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
