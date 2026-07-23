namespace Kritikos.PagedResults.Tests;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class OffsetPagedResultExtensionsTests
{
  [Test]
  public async Task ToOffsetPaged_returns_the_requested_page()
  {
    using var context = CreateContext(25);

    var result = context.Widgets.OrderBy(static w => w.Id).ToOffsetPaged(page: 2, pageSize: 10);

    await Assert.That(result.CurrentPage).IsEqualTo(2);
    await Assert.That(result.PageSize).IsEqualTo(10);
    await Assert.That(result.TotalCount).IsEqualTo(25);
    await Assert.That(result.TotalPages).IsEqualTo(3);
    await Assert.That(result.Items.Count).IsEqualTo(10);
    await Assert.That(result.Items[0].Id).IsEqualTo(11);
    await Assert.That(result.HasPreviousPage).IsTrue();
    await Assert.That(result.HasNextPage).IsTrue();
  }

  [Test]
  public async Task ToOffsetPagedAsync_returns_the_last_partial_page(CancellationToken cancellationToken)
  {
    using var context = CreateContext(25);

    var result = await context.Widgets.OrderBy(static w => w.Id)
      .ToOffsetPagedAsync(page: 3, pageSize: 10, cancellationToken);

    await Assert.That(result.TotalCount).IsEqualTo(25);
    await Assert.That(result.TotalPages).IsEqualTo(3);
    await Assert.That(result.Items.Count).IsEqualTo(5);
    await Assert.That(result.Items[0].Id).IsEqualTo(21);
    await Assert.That(result.HasNextPage).IsFalse();
  }

  [Test]
  public async Task ToOffsetPaged_projects_each_item()
  {
    using var context = CreateContext(5);

    var result = context.Widgets.OrderBy(static w => w.Id)
      .ToOffsetPaged(static w => new WidgetName(w.Name), page: 1, pageSize: 3);

    await Assert.That(result.TotalCount).IsEqualTo(5);
    await Assert.That(result.Items.Count).IsEqualTo(3);
    await Assert.That(result.Items[0].Name).IsEqualTo("w1");
  }

  [Test]
  public async Task ToOffsetPagedAsync_projects_each_item(CancellationToken cancellationToken)
  {
    using var context = CreateContext(5);

    var result = await context.Widgets.OrderBy(static w => w.Id)
      .ToOffsetPagedAsync(static w => new WidgetName(w.Name), page: 1, pageSize: 3, cancellationToken);

    await Assert.That(result.Items.Count).IsEqualTo(3);
    await Assert.That(result.Items[2].Name).IsEqualTo("w3");
  }

  [Test]
  public async Task ToOffsetPaged_rejects_a_non_positive_page()
  {
    using var context = CreateContext(5);

    await Assert.That(() => context.Widgets.OrderBy(static w => w.Id).ToOffsetPaged(page: 0, pageSize: 10))
      .Throws<ArgumentOutOfRangeException>();
  }

  [Test]
  public async Task ToOffsetPaged_rejects_a_non_positive_page_size()
  {
    using var context = CreateContext(5);

    await Assert.That(() => context.Widgets.OrderBy(static w => w.Id).ToOffsetPaged(page: 1, pageSize: 0))
      .Throws<ArgumentOutOfRangeException>();
  }

  private static PagingDbContext CreateContext(int itemCount)
  {
    var context = new PagingDbContext();
    context.Database.EnsureCreated();
    context.Widgets.AddRange(Enumerable.Range(1, itemCount).Select(static i => new Widget { Id = i, Name = $"w{i}" }));
    context.SaveChanges();
    return context;
  }

  private sealed record WidgetName(string Name);

  private sealed class Widget
  {
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
  }

  private sealed class PagingDbContext : DbContext
  {
    private readonly SqliteConnection connection;

    public PagingDbContext()
    {
      connection = new SqliteConnection("DataSource=:memory:");
      connection.Open();
    }

    public DbSet<Widget> Widgets => Set<Widget>();

    public override void Dispose()
    {
      base.Dispose();
      connection.Dispose();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      => optionsBuilder.UseSqlite(connection);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
      => modelBuilder.Entity<Widget>().Property(static w => w.Id).ValueGeneratedNever();
  }
}
