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
}
