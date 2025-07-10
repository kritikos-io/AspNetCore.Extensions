namespace Kritikos.PagedResults;

public sealed class OffsetPagedResult<T> : PagedResult<T>
    where T : class
{
  public int CurrentPage { get; init; }

  public int PageSize { get; init; }

  public int TotalPages { get; init; }

  public int TotalCount { get; init; }

  public bool HasPreviousPage => CurrentPage > 1;

  public bool HasNextPage => CurrentPage < TotalPages;
}
