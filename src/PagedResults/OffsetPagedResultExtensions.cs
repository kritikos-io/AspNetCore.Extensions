namespace Kritikos.PagedResults;

using System.Linq.Expressions;

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

  public static OffsetPagedResult<TDestination> ToOffsetPaged<TSource, TDestination>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TDestination>> mapper, int page, int pageSize)
      where TSource : class
      where TDestination : class
  {
    if (!source.TryGetNonEnumeratedCount(out var count))
    {
      count = source.Count();
    }

    var pageCount = (int)Math.Ceiling((double)count / pageSize);
    var items = source.Skip((page - 1) * pageSize).Take(pageSize).Select(mapper);

    var result = new OffsetPagedResult<TDestination>
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

  public static async Task<OffsetPagedResult<TDestination>> ToOffsetPagedAsync<TSource, TDestination>(
      this IOrderedQueryable<TSource> source,
      Expression<Func<TSource, TDestination>> mapper,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
      where TSource : class
      where TDestination : class
  {
    if (!source.TryGetNonEnumeratedCount(out var count))
    {
      count = await source.CountAsync(cancellationToken);
    }

    var pageCount = (int)Math.Ceiling((double)count / pageSize);
    var items = await source.Skip((page - 1) * pageSize).Take(pageSize).Select(mapper).ToArrayAsync(cancellationToken);

    var result = new OffsetPagedResult<TDestination>
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
