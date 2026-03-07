namespace WesNews.Application.DTOs;

public class PagedResult<T>(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
{
    public IReadOnlyList<T> Items { get; init; } = items;
    public int TotalCount { get; init; } = totalCount;
    public int Page { get; init; } = page;
    public int PageSize { get; init; } = pageSize;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
