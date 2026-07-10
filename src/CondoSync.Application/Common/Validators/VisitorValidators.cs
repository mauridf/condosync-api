using FluentValidation;
using CondoSync.Application.Features.Visitors.DTOs;

namespace CondoSync.Application.Common.Validators;

public class AuthorizeVisitorRequestValidator : AbstractValidator<AuthorizeVisitorRequest>
{
    public AuthorizeVisitorRequestValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.VisitDate).NotEmpty();
        RuleFor(x => x.VisitorType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
