using MediatR;
using Microsoft.Extensions.Logging;
using CondoSync.Core.Interfaces;

namespace CondoSync.Application.Common.Behaviors;

public class TenantBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<TenantBehavior<TRequest, TResponse>> _logger;

    public TenantBehavior(ITenantProvider tenantProvider, ILogger<TenantBehavior<TRequest, TResponse>> logger)
    {
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetCurrentTenantId();
        if (tenantId.HasValue)
        {
            _logger.LogDebug("Processando requisição para tenant {TenantId}", tenantId.Value);
        }

        return await next();
    }
}
