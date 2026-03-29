namespace Kritikos.PagedResults;

#pragma warning disable CA1034 // Do not nest type - false positive from C# 14 extension blocks

/// <summary>
/// Extension methods for <see cref="PagedResult{T}"/>.
/// </summary>
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
