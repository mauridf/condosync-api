using CondoSync.Core.Enums;
using CondoSync.Core.Events;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Notice : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid AuthorId { get; private set; }

    public string Title { get; private set; }
    public string Content { get; private set; }
    public string? Summary { get; private set; }
    public NoticeCategory Category { get; private set; }

    // Visibilidade
    public string Visibility { get; private set; }
    public string? TargetUnits { get; private set; }

    // Destaque
    public bool IsPinned { get; private set; }
    public bool IsUrgent { get; private set; }
    public DateTime? PinExpiresAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Engajamento
    public int ViewsCount { get; private set; }
    public int UniqueViewsCount { get; private set; }

    // Arquivos
    public string? Attachments { get; private set; }

    // Reações
    public string? Reactions { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Notice() { }

    public static Notice Create(
        Guid condominiumId,
        Guid authorId,
        string title,
        string content,
        NoticeCategory category = NoticeCategory.General,
        string? summary = null,
        bool isUrgent = false,
        string visibility = "all")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título do aviso não pode ser vazio");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Conteúdo do aviso não pode ser vazio");

        return new Notice
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            AuthorId = authorId,
            Title = title,
            Content = content,
            Summary = summary,
            Category = category,
            IsUrgent = isUrgent,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Publish()
    {
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NoticePublishedEvent(Id, Category.ToString(), IsUrgent));
    }

    public void Update(string title, string content, string? summary = null,
        NoticeCategory? category = null, bool? isUrgent = null)
    {
        Title = title;
        Content = content;
        if (summary != null) Summary = summary;
        if (category.HasValue) Category = category.Value;
        if (isUrgent.HasValue) IsUrgent = isUrgent.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pin(DateTime? expiresAt = null)
    {
        IsPinned = true;
        PinExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpin()
    {
        IsPinned = false;
        PinExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementView()
    {
        ViewsCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementUniqueView()
    {
        UniqueViewsCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddReaction(string reactionType)
    {
        var reactions = string.IsNullOrEmpty(Reactions)
            ? new Dictionary<string, int>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(Reactions);

        if (reactions!.ContainsKey(reactionType))
            reactions[reactionType]++;
        else
            reactions[reactionType] = 1;

        Reactions = System.Text.Json.JsonSerializer.Serialize(reactions);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveReaction(string reactionType)
    {
        if (string.IsNullOrEmpty(Reactions)) return;

        var reactions = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(Reactions);

        if (reactions!.ContainsKey(reactionType) && reactions[reactionType] > 0)
        {
            reactions[reactionType]--;
            Reactions = System.Text.Json.JsonSerializer.Serialize(reactions);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}