namespace Kritikos.PagedResults;

using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

#pragma warning disable CA1034 // Do not nest type - false positive from C# 14 extension blocks

/// <summary>
/// Extension methods for creating <see cref="OffsetPagedResult{T}"/> from ordered queryable sources.
/// </summary>
public static class OffsetPagedResultExtensions
{
  extension<T>(IOrderedQueryable<T> source)
    where T : class
  {
    /// <summary>
    /// Converts an ordered queryable source into an <see cref="OffsetPagedResult{T}"/>.
    /// </summary>
    /// <param name="page">The 1-based page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="OffsetPagedResult{T}"/> containing the requested page of results.</returns>
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

    /// <summary>
    /// Asynchronously converts an ordered queryable source into an <see cref="OffsetPagedResult{T}"/>.
    /// </summary>
    /// <param name="page">The 1-based page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the requested page of results.</returns>
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

    /// <summary>
    /// Converts an ordered queryable source into an <see cref="OffsetPagedResult{TDestination}"/> using a projection mapper.
    /// </summary>
    /// <typeparam name="TDestination">The type to project each element into.</typeparam>
    /// <param name="mapper">An expression to project each source element into <typeparamref name="TDestination"/>.</param>
    /// <param name="page">The 1-based page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="OffsetPagedResult{TDestination}"/> containing the projected page of results.</returns>
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

    /// <summary>
    /// Asynchronously converts an ordered queryable source into an <see cref="OffsetPagedResult{TDestination}"/> using a projection mapper.
    /// </summary>
    /// <typeparam name="TDestination">The type to project each element into.</typeparam>
    /// <param name="mapper">An expression to project each source element into <typeparamref name="TDestination"/>.</param>
    /// <param name="page">The 1-based page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the projected page of results.</returns>
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
