namespace Kritikos.PetStore.WebApi;

using System.ComponentModel.DataAnnotations;

public record CreateFooDto(
    [property: Required]
    [property: MinLength(3)]
    [property: EmailAddress]
    string Name,
    [property: MinLength(8)]
    [property: Required]
    [property: Phone]
    string Phone,
    [property: Required]
    [property: AllowedValues(1, 2, 3, 4)]
    int Age);
