using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.Documents.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class DocumentService
{
    private readonly IRepository<Document> _documentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IRepository<Document> documentRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IStorageService storageService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _storageService = storageService;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<DocumentResponse>> GetDocumentsAsync(
        string? documentType = null, string? visibility = null, bool? isActive = null)
    {
        var tenantId = GetCurrentTenantId();

        var documents = await _documentRepository.FindAsync(d =>
            d.CondominiumId == tenantId && !d.DeletedAt.HasValue);

        var query = documents.AsQueryable();

        if (!string.IsNullOrEmpty(documentType))
            query = query.Where(d => d.DocumentType.ToString() == documentType);

        if (!string.IsNullOrEmpty(visibility))
            query = query.Where(d => d.Visibility == visibility);

        if (isActive.HasValue)
            query = query.Where(d => d.IsActive == isActive.Value);

        return query.OrderByDescending(d => d.CreatedAt).Select(MapToResponse).ToList();
    }

    public async Task<DocumentDetailResponse?> GetDocumentByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var documents = await _documentRepository.FindAsync(d =>
            d.Id == id && d.CondominiumId == tenantId && !d.DeletedAt.HasValue);
        var document = documents.FirstOrDefault();

        if (document == null) return null;

        var allDocs = await _documentRepository.FindAsync(d =>
            d.CondominiumId == tenantId && !d.DeletedAt.HasValue);

        var versionHistory = BuildVersionHistory(document, allDocs.ToList());

        return new DocumentDetailResponse(
            MapToResponse(document),
            versionHistory.OrderBy(v => v.Version).ToList());
    }

    public async Task<DocumentResponse> UploadDocumentAsync(
        Guid uploadedBy, Stream fileStream, string fileName, string contentType, int fileSize, UploadDocumentRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var documentType = Enum.Parse<DocumentType>(request.DocumentType, true);

        var objectName = $"{tenantId:N}/documents/{Guid.NewGuid():N}/{fileName}";

        var filePath = await _storageService.UploadAsync(
            $"condosync-{tenantId:N}", objectName, fileStream, contentType);

        var document = Document.Create(
            tenantId, uploadedBy, request.Name,
            fileName, filePath, contentType, fileSize,
            documentType, request.Description, request.Visibility);

        if (request.ExpiresAt.HasValue)
            document.SetExpiration(request.ExpiresAt.Value);

        await _documentRepository.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(document);
    }

    public async Task<DocumentResponse?> UpdateDocumentAsync(Guid id, UpdateDocumentRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var documents = await _documentRepository.FindAsync(d =>
            d.Id == id && d.CondominiumId == tenantId && !d.DeletedAt.HasValue);
        var document = documents.FirstOrDefault();

        if (document == null) return null;

        if (request.ExpiresAt.HasValue)
            document.SetExpiration(request.ExpiresAt.Value);

        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(document);
    }

    public async Task<DocumentResponse?> UploadNewVersionAsync(Guid id, Stream fileStream, string fileName, string contentType, int fileSize)
    {
        var tenantId = GetCurrentTenantId();
        var documents = await _documentRepository.FindAsync(d =>
            d.Id == id && d.CondominiumId == tenantId && !d.DeletedAt.HasValue);
        var document = documents.FirstOrDefault();

        if (document == null) return null;

        var objectName = $"{tenantId:N}/documents/{Guid.NewGuid():N}/v{document.Version + 1}_{fileName}";

        var filePath = await _storageService.UploadAsync(
            $"condosync-{tenantId:N}", objectName, fileStream, contentType);

        document.CreateNewVersion(fileName, filePath, contentType, fileSize);

        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(document);
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocumentAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var documents = await _documentRepository.FindAsync(d =>
            d.Id == id && d.CondominiumId == tenantId && !d.DeletedAt.HasValue);
        var document = documents.FirstOrDefault();

        if (document == null) return null;

        var bucketName = $"condosync-{tenantId:N}";
        var uri = new Uri(document.FilePath);
        var objectName = uri.AbsolutePath.TrimStart('/');

        var stream = await _storageService.DownloadAsync(bucketName, objectName);

        return (stream, document.ContentType, document.FileName);
    }

    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var documents = await _documentRepository.FindAsync(d =>
            d.Id == id && d.CondominiumId == tenantId && !d.DeletedAt.HasValue);
        var document = documents.FirstOrDefault();

        if (document == null) return false;

        document.SoftDelete();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static DocumentResponse MapToResponse(Document d)
    {
        return new DocumentResponse(
            d.Id, d.Name, d.Description, d.DocumentType.ToString(),
            d.FileName, d.ContentType, d.FileSize, d.Version,
            d.Visibility, d.IsActive, d.RequiresSignature,
            d.DocumentDate, d.ExpiresAt, d.UploadedBy,
            d.CreatedAt, d.UpdatedAt);
    }

    private static List<DocumentVersionResponse> BuildVersionHistory(Document document, List<Document> allDocs)
    {
        var versions = new List<DocumentVersionResponse>();
        var current = document;

        while (current != null)
        {
            versions.Add(new DocumentVersionResponse(
                current.Id, current.Version, current.FileName,
                current.ContentType, current.FileSize, current.CreatedAt));

            current = current.PreviousVersionId.HasValue
                ? allDocs.FirstOrDefault(d => d.Id == current.PreviousVersionId.Value)
                : null;
        }

        return versions;
    }
}
