namespace Kritikos.PagedResults;

public class PagedResult<T>
    where T : class
{
  public IReadOnlyList<T> Items { get; init; } = [];
}
