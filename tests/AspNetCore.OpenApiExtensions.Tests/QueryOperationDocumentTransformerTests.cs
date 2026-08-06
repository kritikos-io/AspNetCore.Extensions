namespace Kritikos.AspNetCore.OpenApiExtensions.Tests;

using Kritikos.AspNetCore.OpenApiExtensions.DocumentTransformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using NSubstitute;

public class QueryOperationDocumentTransformerTests
{
  [Test]
  public async Task TransformAsync_null_document_throws()
  {
    var transformer = new QueryOperationDocumentTransformer();

    await Assert.That(async () =>
        await transformer.TransformAsync(null!, CreateContext(), CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task TransformAsync_null_context_throws()
  {
    var transformer = new QueryOperationDocumentTransformer();

    await Assert.That(async () =>
        await transformer.TransformAsync(new OpenApiDocument(), null!, CancellationToken.None))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task TransformAsync_without_query_descriptions_leaves_document_untouched()
  {
    var transformer = new QueryOperationDocumentTransformer();
    var document = new OpenApiDocument();

    await transformer.TransformAsync(document, CreateContext(), CancellationToken.None);

    await Assert.That(document.Paths is null || document.Paths.Count == 0).IsTrue();
  }

  [Test]
  public async Task NormalizePath_strips_constraints_and_trailing_slash()
  {
    await Assert.That(QueryOperationDocumentTransformer.NormalizePath("api/v1/pet/{id:long}"))
      .IsEqualTo("/api/v1/pet/{id}");
    await Assert.That(QueryOperationDocumentTransformer.NormalizePath("api/v1/probe/"))
      .IsEqualTo("/api/v1/probe");
    await Assert.That(QueryOperationDocumentTransformer.NormalizePath("api/v1/pet"))
      .IsEqualTo("/api/v1/pet");
    await Assert.That(QueryOperationDocumentTransformer.NormalizePath(string.Empty))
      .IsEqualTo("/");
    await Assert.That(QueryOperationDocumentTransformer.NormalizePath("api/v1/items/{id:guid?}"))
      .IsEqualTo("/api/v1/items/{id}");
  }

  [Test]
  public async Task TryGetElementType_identifies_collections_and_excludes_scalars_strings_and_maps()
  {
    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(int[]), out var array)).IsTrue();
    await Assert.That(array).IsEqualTo(typeof(int));

    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(List<Guid>), out var list)).IsTrue();
    await Assert.That(list).IsEqualTo(typeof(Guid));

    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(IEnumerable<string>), out var seq))
      .IsTrue();
    await Assert.That(seq).IsEqualTo(typeof(string));

    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(string), out _)).IsFalse();
    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(byte[]), out _)).IsFalse();
    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(int), out _)).IsFalse();
    await Assert.That(QueryOperationDocumentTransformer.TryGetElementType(typeof(Dictionary<string, int>), out _))
      .IsFalse();
  }

  private static OpenApiDocumentTransformerContext CreateContext()
    => new()
    {
      DocumentName = "v1",
      DescriptionGroups = [],
      ApplicationServices = Substitute.For<IServiceProvider>(),
    };
}
