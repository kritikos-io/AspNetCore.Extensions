namespace Kritikos.PagedResults;

public static class PagedResultExtensions
{
  /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
  public static IEnumerator<T> GetEnumerator<T>(this PagedResult<T> source)
      where T : class
  {
    ArgumentNullException.ThrowIfNull(source);

    return source.Items.GetEnumerator();
  }

  public static OffsetPagedResult<T> ToOffsetPaged<T>(this IOrderedQueryable<T> source, int page, int pageSize)
      where T : class
  {
    if (!source.TryGetNonEnumeratedCount(out var count))
    {
      count = source.Count();
    }

    var pageCount = (int)Math.Ceiling((double)count / pageSize);
    var items = source.Skip((page - 1) * pageSize).Take(pageSize);

    var result = new OffsetPagedResult<T>
    {
      Items = [.. items],
      CurrentPage = page,
      PageSize = pageSize,
      TotalPages = pageCount,
      TotalCount = count,
    };

    return result;
  }
}
