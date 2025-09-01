namespace Kritikos.PetStore.WebApi.Models.V2;

using System.ComponentModel.DataAnnotations;

public record PetV2Dto(
  [property: Required]
  [property: MinLength(3)]
  string FirstName,
  [property: EmailAddress] string LastName,
  int Age);
