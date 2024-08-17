namespace Kritikos.AspNetCore.MinimalApiExtensions.Filters;

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Adds a validation Filter to the endpoint pipeline.
/// </summary>
/// <param name="isServiceProvider"><see cref="IServiceProviderIsService"/>.</param>
/// <remarks>WIP, performance is not yet rated, use at your own risk.</remarks>
public class ValidatorFilter(IServiceProviderIsService isServiceProvider) : IEndpointFilter
{
  private readonly IServiceProviderIsService isServiceProvider = isServiceProvider;

  /// <inheritdoc />
  public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(next);

    var validationResult = new Collection<ValidationResult>();

    foreach (var argument in context.Arguments.Where(x => x is not null && x.GetType().IsClass && !isServiceProvider.IsService(x.GetType())))
    {
      var type = argument?.GetType();
      if (type is null || argument is null)
      {
        continue;
      }

      var ctx = new ValidationContext(argument, null, null);

      if (Validator.TryValidateObject(argument, ctx, validationResult, true))
      {
        continue;
      }

      var errors = validationResult.GroupBy(x => x.MemberNames.First())
          .ToDictionary(
              x => x.Key,
              x => x.Select(y => y.ErrorMessage ?? string.Empty)
                  .ToArray());
      return Results.ValidationProblem(errors);
    }

    return await next(context);
  }
}
