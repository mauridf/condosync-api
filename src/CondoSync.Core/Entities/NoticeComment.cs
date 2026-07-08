using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class NoticeComment : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid NoticeId { get; private set; }
    public Guid AuthorId { get; private set; }

    public string Content { get; private set; }
    public bool IsEdited { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private NoticeComment() { }

    public static NoticeComment Create(Guid condominiumId, Guid noticeId, Guid authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Comentário não pode ser vazio");

        return new NoticeComment
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            NoticeId = noticeId,
            AuthorId = authorId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Edit(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new DomainException("Comentário não pode ser vazio");

        Content = newContent;
        IsEdited = true;
        UpdatedAt = DateTime.UtcNow;
    }
}