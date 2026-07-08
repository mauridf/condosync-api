namespace CondoSync.Core.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode = "DOMAIN_ERROR")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, Exception innerException, string errorCode = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message = "Recurso não encontrado")
        : base(message, "NOT_FOUND")
    {
    }
}

public class ValidationException : DomainException
{
    public List<ValidationError> Errors { get; }

    public ValidationException(string message, List<ValidationError>? errors = null)
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors ?? new List<ValidationError>();
    }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Acesso não autorizado")
        : base(message, "UNAUTHORIZED")
    {
    }
}

public class ConflictException : DomainException
{
    public ConflictException(string message = "Conflito de dados")
        : base(message, "CONFLICT")
    {
    }
}

public class TenantNotFoundException : DomainException
{
    public TenantNotFoundException(string message = "Condomínio não encontrado")
        : base(message, "TENANT_NOT_FOUND")
    {
    }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ValidationError() { }

    public ValidationError(string field, string message)
    {
        Field = field;
        Message = message;
    }
}