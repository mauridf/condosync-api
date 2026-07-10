namespace CondoSync.Application.Features.Documents.DTOs;

public record UploadDocumentRequest(
    string Name,
    string? Description,
    string DocumentType = "Other",
    string Visibility = "all",
    DateOnly? DocumentDate = null,
    DateOnly? ExpiresAt = null,
    bool RequiresSignature = false
);

public record UpdateDocumentRequest(
    string? Name,
    string? Description,
    string? DocumentType,
    string? Visibility,
    DateOnly? DocumentDate,
    DateOnly? ExpiresAt,
    bool? RequiresSignature
);

public record DocumentResponse(
    Guid Id,
    string Name,
    string? Description,
    string DocumentType,
    string FileName,
    string ContentType,
    int FileSize,
    int Version,
    string Visibility,
    bool IsActive,
    bool RequiresSignature,
    DateOnly? DocumentDate,
    DateOnly? ExpiresAt,
    Guid UploadedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record DocumentVersionResponse(
    Guid Id,
    int Version,
    string FileName,
    string ContentType,
    int FileSize,
    DateTime CreatedAt
);

public record DocumentDetailResponse(
    DocumentResponse Document,
    List<DocumentVersionResponse> Versions
);
