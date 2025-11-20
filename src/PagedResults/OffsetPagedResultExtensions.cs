namespace Kritikos.PagedResults;

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

public static class OffsetPagedResultExtensions
{
  extension<T>(IOrderedQueryable<T> source)
    where T : class
  {
    public OffsetPagedResult<T> ToOffsetPaged(int page, int pageSize)
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

    public async Task<OffsetPagedResult<T>> ToOffsetPagedAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
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

    public OffsetPagedResult<TDestination> ToOffsetPaged<TDestination>(
      Expression<Func<T, TDestination>> mapper,
      int page,
      int pageSize)
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

    public async Task<OffsetPagedResult<TDestination>> ToOffsetPagedAsync<TDestination>(
      Expression<Func<T, TDestination>> mapper,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
      where TDestination : class
    {
      if (!source.TryGetNonEnumeratedCount(out var count))
      {
        count = await source.CountAsync(cancellationToken);
      }

      var pageCount = (int)Math.Ceiling((double)count / pageSize);
      var items = await source.Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(mapper)
        .ToArrayAsync(cancellationToken);

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
}
