using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Document : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UploadedBy { get; private set; }

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public CondoSync.Core.Enums.DocumentType DocumentType { get; private set; }

    // Arquivo
    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public string ContentType { get; private set; }
    public int FileSize { get; private set; }

    // Versão
    public int Version { get; private set; }
    public Guid? PreviousVersionId { get; private set; }

    // Visibilidade
    public string Visibility { get; private set; }

    // Datas
    public DateOnly? DocumentDate { get; private set; }
    public DateOnly? ExpiresAt { get; private set; }

    // Status
    public bool IsActive { get; private set; }
    public bool RequiresSignature { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Document() { }

    public static Document Create(
        Guid condominiumId,
        Guid uploadedBy,
        string name,
        string fileName,
        string filePath,
        string contentType,
        int fileSize,
        CondoSync.Core.Enums.DocumentType documentType = CondoSync.Core.Enums.DocumentType.Other,
        string? description = null,
        string visibility = "all")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do documento não pode ser vazio");

        return new Document
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UploadedBy = uploadedBy,
            Name = name,
            Description = description,
            DocumentType = documentType,
            FileName = fileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSize = fileSize,
            Version = 1,
            Visibility = visibility,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void CreateNewVersion(string fileName, string filePath, string contentType, int fileSize)
    {
        PreviousVersionId = Id;
        Version++;
        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExpiration(DateOnly expiresAt)
    {
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}