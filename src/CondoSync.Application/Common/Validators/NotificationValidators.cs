using FluentValidation;
using CondoSync.Application.Features.Notifications.DTOs;

namespace CondoSync.Application.Common.Validators;

public class CreateNotificationTemplateRequestValidator : AbstractValidator<CreateNotificationTemplateRequest>
{
    public CreateNotificationTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TitleTemplate).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BodyTemplate).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.NotificationType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateNotificationTemplateRequestValidator : AbstractValidator<UpdateNotificationTemplateRequest>
{
    public UpdateNotificationTemplateRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name != null);
        RuleFor(x => x.TitleTemplate).MaximumLength(500).When(x => x.TitleTemplate != null);
        RuleFor(x => x.BodyTemplate).MaximumLength(2000).When(x => x.BodyTemplate != null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}
