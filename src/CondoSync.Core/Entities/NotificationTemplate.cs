namespace CondoSync.Core.Entities;

public class NotificationTemplate : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string TitleTemplate { get; private set; } = default!;
    public string BodyTemplate { get; private set; } = default!;
    public string NotificationType { get; private set; } = default!;

    public string Channel { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        Guid condominiumId, string name, string titleTemplate, string bodyTemplate,
        string notificationType, string channel = "in_app", string? description = null)
    {
        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            Name = name,
            Description = description,
            TitleTemplate = titleTemplate,
            BodyTemplate = bodyTemplate,
            NotificationType = notificationType,
            Channel = channel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? name, string? titleTemplate, string? bodyTemplate, string? description = null)
    {
        if (name != null) Name = name;
        if (titleTemplate != null) TitleTemplate = titleTemplate;
        if (bodyTemplate != null) BodyTemplate = bodyTemplate;
        if (description != null) Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public string RenderTitle(Dictionary<string, string> variables)
    {
        return Render(TitleTemplate, variables);
    }

    public string RenderBody(Dictionary<string, string> variables)
    {
        return Render(BodyTemplate, variables);
    }

    private static string Render(string template, Dictionary<string, string> variables)
    {
        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
        return result;
    }
}
