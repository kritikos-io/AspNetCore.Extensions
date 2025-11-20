namespace Kritikos.PetStore.WebApi;

public record VersionDto(ICollection<string> SupportedVersions, ICollection<string> DeprecatedVersions);
