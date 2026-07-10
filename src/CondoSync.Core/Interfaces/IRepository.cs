using CondoSync.Core.Common;
using CondoSync.Core.Enums;
using System.Linq.Expressions;

namespace CondoSync.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<PaginatedResult<T>> GetPagedAsync(int page, int perPage,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    IQueryable<T> Query();
}

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public interface ITenantProvider
{
    Guid? GetCurrentTenantId();
    string? GetCurrentTenantSlug();
    void SetCurrentTenant(Guid tenantId, string slug);
}

public interface ICurrentUserService
{
    Guid? GetUserId();
    Guid? GetTenantId();
    string? GetUserRole();
    bool IsAuthenticated();
    bool IsSuperAdmin();
}

public interface IStorageService
{
    Task<string> UploadAsync(string bucketName, string objectName, Stream data, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
}

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string routingKey = "", CancellationToken cancellationToken = default) where T : class;
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string body, NotificationType type,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task SendToAllAsync(IEnumerable<Guid> userIds, string title, string body, NotificationType type,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
}

public interface IPaymentGateway
{
    Task<string> GenerateBoletoAsync(decimal amount, string description, DateTime dueDate,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<string> GeneratePixAsync(decimal amount, string description,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<bool> ProcessPaymentAsync(string transactionId, PaymentMethod method, CancellationToken cancellationToken = default);
}