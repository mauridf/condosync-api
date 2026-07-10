using FluentValidation;
using CondoSync.Application.Features.Invitations.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateInvitationRequestValidator : AbstractValidator<CreateInvitationRequest>
{
    public CreateInvitationRequestValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.RecipientEmail).EmailAddress().When(x => x.RecipientEmail != null);
        RuleFor(x => x.RecipientName).MaximumLength(200);
        RuleFor(x => x.RecipientPhone).MaximumLength(20);
        RuleFor(x => x.AccessType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.MaxUses).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.ValidityDays).GreaterThan(0).LessThanOrEqualTo(365);
    }
}

public class UseInvitationRequestValidator : AbstractValidator<UseInvitationRequest>
{
    public UseInvitationRequestValidator()
    {
        RuleFor(x => x.InvitationCode).NotEmpty();
    }
}
