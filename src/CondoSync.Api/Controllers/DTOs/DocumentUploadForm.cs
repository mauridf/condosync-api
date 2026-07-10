namespace CondoSync.Api.Controllers.DTOs;

public class DocumentUploadForm
{
    public IFormFile File { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = "Other";
    public string Visibility { get; set; } = "all";
    public DateOnly? DocumentDate { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public bool RequiresSignature { get; set; }
}
