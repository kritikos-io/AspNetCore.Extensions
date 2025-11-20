namespace Kritikos.PagedResults;

/// <summary>
/// A <see cref="PagedResult{T}"/> that uses offset and provides page information.
/// </summary>
/// <typeparam name="T">The type of elements in the result.</typeparam>
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
