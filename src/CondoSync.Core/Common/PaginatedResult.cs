namespace CondoSync.Core.Common;

public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PerPage);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PaginatedResult<T> Create(List<T> data, int total, int page, int perPage)
    {
        return new PaginatedResult<T>
        {
            Data = data,
            Total = total,
            Page = page,
            PerPage = perPage
        };
    }
}
