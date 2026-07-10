using FluentValidation;
using CondoSync.Application.Features.Employees.DTOs;
using CondoSync.Core.Enums;

namespace CondoSync.Application.Common.Validators;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone));
        RuleFor(x => x.Document).MaximumLength(14).When(x => !string.IsNullOrEmpty(x.Document));
        RuleFor(x => x.Role).NotEmpty().MaximumLength(30)
            .Must(v => Enum.TryParse<UserRole>(v, true, out var r)
                && r is UserRole.Employee or UserRole.SubAdmin)
            .WithMessage("Cargo deve ser Employee ou SubAdmin");
    }
}
