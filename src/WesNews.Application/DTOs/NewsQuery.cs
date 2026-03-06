using WesNews.Domain.Enums;

namespace WesNews.Application.DTOs;

public class NewsQuery
{
    public Category? Category { get; init; }
    public string? Search { get; init; }
    public bool UnreadOnly { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
