using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class NoticeService
{
    private readonly IRepository<Notice> _noticeRepository;
    private readonly IRepository<NoticeComment> _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<NoticeService> _logger;

    public NoticeService(
        IRepository<Notice> noticeRepository,
        IRepository<NoticeComment> commentRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<NoticeService> logger)
    {
        _noticeRepository = noticeRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Notice>> GetNoticesAsync(
        string? category = null, bool? isPinned = null, int page = 1, int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var notices = await _noticeRepository.FindAsync(n => n.CondominiumId == tenantId);

        var query = notices.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var noticeCategory = Enum.Parse<NoticeCategory>(category, true);
            query = query.Where(n => n.Category == noticeCategory);
        }

        if (isPinned.HasValue)
            query = query.Where(n => n.IsPinned == isPinned.Value);

        return query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.PublishedAt ?? n.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Notice?> GetNoticeByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var notices = await _noticeRepository.FindAsync(n => n.Id == id && n.CondominiumId == tenantId);
        var notice = notices.FirstOrDefault();

        if (notice != null)
        {
            notice.IncrementView();
            await _unitOfWork.SaveChangesAsync();
        }

        return notice;
    }

    public async Task<Notice> CreateNoticeAsync(
        Guid authorId, string title, string content,
        string category = "General", bool isUrgent = false,
        string? summary = null, string visibility = "all")
    {
        var tenantId = GetCurrentTenantId();
        var noticeCategory = Enum.Parse<NoticeCategory>(category, true);

        var notice = Notice.Create(
            tenantId, authorId, title, content,
            noticeCategory, summary, isUrgent, visibility);

        notice.Publish();

        await _noticeRepository.AddAsync(notice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Aviso publicado: {Title}", title);

        return notice;
    }

    public async Task<Notice?> UpdateNoticeAsync(Guid id, string title, string content,
        string? summary = null, string? category = null, bool? isUrgent = null)
    {
        var notice = await GetNoticeByIdAsync(id);
        if (notice == null) return null;

        NoticeCategory? noticeCategory = null;
        if (!string.IsNullOrWhiteSpace(category))
            noticeCategory = Enum.Parse<NoticeCategory>(category, true);

        notice.Update(title, content, summary, noticeCategory, isUrgent);
        await _unitOfWork.SaveChangesAsync();

        return notice;
    }

    public async Task<bool> TogglePinAsync(Guid id)
    {
        var notice = await GetNoticeByIdAsync(id);
        if (notice == null) return false;

        if (notice.IsPinned)
            notice.Unpin();
        else
            notice.Pin();

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNoticeAsync(Guid id)
    {
        var notice = await GetNoticeByIdAsync(id);
        if (notice == null) return false;

        notice.SoftDelete();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<NoticeComment> AddCommentAsync(Guid noticeId, Guid authorId, string content)
    {
        var tenantId = GetCurrentTenantId();
        var notice = await GetNoticeByIdAsync(noticeId);

        if (notice == null)
            throw new InvalidOperationException("Aviso não encontrado");

        var comment = NoticeComment.Create(tenantId, noticeId, authorId, content);
        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        return comment;
    }

    public async Task<bool> AddReactionAsync(Guid noticeId, string reactionType)
    {
        var notice = await GetNoticeByIdAsync(noticeId);
        if (notice == null) return false;

        notice.AddReaction(reactionType);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveReactionAsync(Guid noticeId, string reactionType)
    {
        var notice = await GetNoticeByIdAsync(noticeId);
        if (notice == null) return false;

        notice.RemoveReaction(reactionType);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}