using FluentValidation;
using CondoSync.Application.Features.Notices.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateNoticeRequestValidator : AbstractValidator<CreateNoticeRequest>
{
    public CreateNoticeRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.Category).MaximumLength(50);
        RuleFor(x => x.Summary).MaximumLength(500);
        RuleFor(x => x.Visibility).MaximumLength(30);
    }
}

public class UpdateNoticeRequestValidator : AbstractValidator<UpdateNoticeRequest>
{
    public UpdateNoticeRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.Summary).MaximumLength(500);
    }
}

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public class AddReactionRequestValidator : AbstractValidator<AddReactionRequest>
{
    public AddReactionRequestValidator()
    {
        RuleFor(x => x.ReactionType).NotEmpty().MaximumLength(50);
    }
}
