using FluentValidation;
using CondoSync.Application.Features.Documents.DTOs;

namespace CondoSync.Application.Common.Validators;

public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    public UploadDocumentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Visibility).NotEmpty().MaximumLength(20);
    }
}

public class UpdateDocumentRequestValidator : AbstractValidator<UpdateDocumentRequest>
{
    public UpdateDocumentRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.DocumentType).MaximumLength(30).When(x => x.DocumentType != null);
        RuleFor(x => x.Visibility).MaximumLength(20).When(x => x.Visibility != null);
    }
}
