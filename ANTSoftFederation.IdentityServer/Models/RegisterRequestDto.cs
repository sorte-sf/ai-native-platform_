namespace ANTSoftFederation.IdentityServer.Models;

public sealed record RegisterRequestDto(
    string Email,
    string Password, 
    string Name,
    string MobilePhone, 
    string? CompanyShortName,
    string? CompanyName,
    string? RealStateCertificateNo,
    Guid? CountryId,
    Guid? StateId,
    Guid? CityId,
    string? Address);

public sealed record RegisterResponse(Guid UserId, string Email, string DisplayName, Guid? OrganizationId);

