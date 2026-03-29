namespace Kritikos.PagedResults;

/// <summary>
/// A <see cref="PagedResult{T}"/> that uses offset and provides page information.
/// </summary>
/// <typeparam name="T">The type of elements in the result.</typeparam>
public sealed class OffsetPagedResult<T> : PagedResult<T>
    where T : class
{
  /// <summary>
  /// Gets the current page number (1-based).
  /// </summary>
  public int CurrentPage { get; init; }

  /// <summary>
  /// Gets the number of items per page.
  /// </summary>
  public int PageSize { get; init; }

  /// <summary>
  /// Gets the total number of pages available.
  /// </summary>
  public int TotalPages { get; init; }

  /// <summary>
  /// Gets the total number of items across all pages.
  /// </summary>
  public int TotalCount { get; init; }

  /// <summary>
  /// Gets a value indicating whether a previous page exists.
  /// </summary>
  public bool HasPreviousPage => CurrentPage > 1;

  /// <summary>
  /// Gets a value indicating whether a next page exists.
  /// </summary>
  public bool HasNextPage => CurrentPage < TotalPages;
}
