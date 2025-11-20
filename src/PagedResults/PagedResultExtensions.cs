namespace Kritikos.PagedResults;

public static class PagedResultExtensions
{
  extension<T>(PagedResult<T> source)
    where T : class
  {
    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public IEnumerator<T> GetEnumerator()
    {
      ArgumentNullException.ThrowIfNull(source);

      return source.Items.GetEnumerator();
    }
  }
}
