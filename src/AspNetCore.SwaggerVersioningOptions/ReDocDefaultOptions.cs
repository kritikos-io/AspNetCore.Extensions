namespace Kritikos.AspNetCore.SwaggerVersioningOptions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.ReDoc;

public class ReDocDefaultOptions : IConfigureOptions<ReDocOptions>
{
  /// <inheritdoc />
  public void Configure(ReDocOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    options.HideHostname();
    options.HideDownloadButton();
    options.ExpandResponses("200,201");
    options.RequiredPropsFirst();
    options.SortPropsAlphabetically();
  }
}
