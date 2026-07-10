using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Documents.DTOs;
using CondoSync.Application.Services;
using CondoSync.Api.Controllers.DTOs;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class DocumentsController : BaseController
{
    private readonly DocumentService _documentService;

    public DocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? documentType = null,
        [FromQuery] string? visibility = null,
        [FromQuery] bool? isActive = null)
    {
        var documents = await _documentService.GetDocumentsAsync(documentType, visibility, isActive);
        return Ok(new { success = true, data = documents });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        return document == null ? NotFound() : Ok(new { success = true, data = document });
    }

    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadForm form)
    {
        if (form.File == null || form.File.Length == 0)
            return BadRequest(new { success = false, message = "Arquivo obrigatório" });

        var userId = GetUserId() ?? throw new UnauthorizedAccessException("Usuário não autenticado");

        var request = new UploadDocumentRequest(
            form.Name, form.Description, form.DocumentType, form.Visibility,
            form.DocumentDate, form.ExpiresAt, form.RequiresSignature);

        using var stream = form.File.OpenReadStream();
        var document = await _documentService.UploadDocumentAsync(
            userId, stream, form.File.FileName, form.File.ContentType, (int)form.File.Length, request);
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, new { success = true, data = document });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var document = await _documentService.UpdateDocumentAsync(id, request);
        return document == null ? NotFound() : Ok(new { success = true, data = document });
    }

    [HttpPost("{id:guid}/versions")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadVersion(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "Arquivo obrigatório" });

        using var stream = file.OpenReadStream();
        var document = await _documentService.UploadNewVersionAsync(
            id, stream, file.FileName, file.ContentType, (int)file.Length);
        return document == null ? NotFound() : Ok(new { success = true, data = document });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await _documentService.DownloadDocumentAsync(id);
        if (result == null) return NotFound();

        var (stream, contentType, fileName) = result.Value;
        return File(stream, contentType, fileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _documentService.DeleteDocumentAsync(id);
        return deleted ? Ok(new { success = true }) : NotFound();
    }
}
