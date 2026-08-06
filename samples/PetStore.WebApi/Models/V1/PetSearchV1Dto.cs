namespace Kritikos.PetStore.WebApi.Models.V1;

public record PetSearchV1Dto(string? NameContains, int? MinAge, int? MaxAge);
