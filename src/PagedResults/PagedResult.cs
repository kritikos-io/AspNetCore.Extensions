namespace Kritikos.PagedResults;

/// <summary>
/// A subset of items representing a single page from a larger whole.
/// </summary>
/// <typeparam name="T">The type of elements in the read-only list.</typeparam>
public class PagedResult<T>
    where T : class
{
  /// <summary>
  /// Gets the items contained in this page.
  /// </summary>
  public IReadOnlyList<T> Items { get; init; } = [];
}
