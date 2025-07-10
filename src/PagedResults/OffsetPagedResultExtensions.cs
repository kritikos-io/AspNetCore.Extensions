namespace Kritikos.PagedResults;

using Microsoft.EntityFrameworkCore;

public static class OffsetPagedResultExtensions
{
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

  public static async Task<OffsetPagedResult<T>> ToOffsetPagedAsync<T>(this IOrderedQueryable<T> source, int page, int pageSize, CancellationToken cancellationToken = default)
      where T : class
  {
    if (!source.TryGetNonEnumeratedCount(out var count))
    {
      count = await source.CountAsync(cancellationToken);
    }

    var pageCount = (int)Math.Ceiling((double)count / pageSize);
    var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);

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
