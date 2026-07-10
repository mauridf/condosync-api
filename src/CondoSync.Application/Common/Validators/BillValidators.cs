using FluentValidation;
using CondoSync.Application.Features.Bills.DTOs;

namespace CondoSync.Application.Common.Validators;

public class GenerateBillRequestValidator : AbstractValidator<GenerateBillRequest>
{
    public GenerateBillRequestValidator()
    {
        RuleFor(x => x.UnitId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ReferenceMonth).NotEmpty().Matches(@"^\d{4}-\d{2}$")
            .WithMessage("Mês de referência deve estar no formato YYYY-MM");
        RuleFor(x => x.BaseAmount).GreaterThan(0);
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.LateFeePercentage).InclusiveBetween(0, 100).When(x => x.LateFeePercentage.HasValue);
        RuleFor(x => x.LateInterestDaily).InclusiveBetween(0, 100).When(x => x.LateInterestDaily.HasValue);
    }
}

public class BatchGenerateBillsRequestValidator : AbstractValidator<BatchGenerateBillsRequest>
{
    public BatchGenerateBillsRequestValidator()
    {
        RuleFor(x => x.ReferenceMonth).NotEmpty().Matches(@"^\d{4}-\d{2}$")
            .WithMessage("Mês de referência deve estar no formato YYYY-MM");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DueDate).NotEmpty();
    }
}

public class PayBillRequestValidator : AbstractValidator<PayBillRequest>
{
    public PayBillRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TransactionId).NotEmpty().MaximumLength(100);
    }
}
