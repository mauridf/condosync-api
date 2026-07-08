namespace CondoSync.Application.Common.DTOs;

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Details { get; set; }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ApiMeta Meta { get; set; } = new();
}

public class ApiResponse
{
    public bool Success { get; set; }
    public ApiMeta Meta { get; set; } = new();
}

public class ApiMeta
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
    public int? Page { get; set; }
    public int? PerPage { get; set; }
    public int? Total { get; set; }
    public int? TotalPages { get; set; }
}