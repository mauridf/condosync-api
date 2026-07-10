using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.Notifications.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class NotificationTemplateService
{
    private readonly IRepository<NotificationTemplate> _templateRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<NotificationTemplateService> _logger;

    public NotificationTemplateService(
        IRepository<NotificationTemplate> templateRepo,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<NotificationTemplateService> logger)
    {
        _templateRepo = templateRepo;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<NotificationTemplateResponse>> GetAllAsync(string? notificationType = null)
    {
        var tenantId = GetCurrentTenantId();
        var templates = await _templateRepo.FindAsync(t => t.CondominiumId == tenantId);

        var query = templates.AsQueryable();
        if (!string.IsNullOrEmpty(notificationType))
            query = query.Where(t => t.NotificationType == notificationType);

        return query.OrderBy(t => t.Name).Select(MapToResponse).ToList();
    }

    public async Task<NotificationTemplateResponse?> GetByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var templates = await _templateRepo.FindAsync(t => t.Id == id && t.CondominiumId == tenantId);
        var template = templates.FirstOrDefault();
        return template == null ? null : MapToResponse(template);
    }

    public async Task<NotificationTemplateResponse> CreateAsync(CreateNotificationTemplateRequest request)
    {
        var tenantId = GetCurrentTenantId();

        var template = NotificationTemplate.Create(
            tenantId, request.Name, request.TitleTemplate, request.BodyTemplate,
            request.NotificationType, request.Channel, request.Description);

        await _templateRepo.AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Template de notificação {TemplateName} criado", request.Name);
        return MapToResponse(template);
    }

    public async Task<NotificationTemplateResponse?> UpdateAsync(Guid id, UpdateNotificationTemplateRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var templates = await _templateRepo.FindAsync(t => t.Id == id && t.CondominiumId == tenantId);
        var template = templates.FirstOrDefault();

        if (template == null) return null;

        template.Update(request.Name, request.TitleTemplate, request.BodyTemplate, request.Description);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(template);
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var templates = await _templateRepo.FindAsync(t => t.Id == id && t.CondominiumId == tenantId);
        var template = templates.FirstOrDefault();

        if (template == null) return false;

        if (template.IsActive)
            template.Deactivate();
        else
            template.Activate();

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var templates = await _templateRepo.FindAsync(t => t.Id == id && t.CondominiumId == tenantId);
        var template = templates.FirstOrDefault();

        if (template == null) return false;

        _templateRepo.Remove(template);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static NotificationTemplateResponse MapToResponse(NotificationTemplate t)
    {
        return new NotificationTemplateResponse(
            t.Id, t.Name, t.Description, t.TitleTemplate, t.BodyTemplate,
            t.NotificationType, t.Channel, t.IsActive, t.CreatedAt, t.UpdatedAt);
    }
}
