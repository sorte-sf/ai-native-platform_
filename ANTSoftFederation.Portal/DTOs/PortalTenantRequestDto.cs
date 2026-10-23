namespace SorteFederation.Portal.DTOs;

public sealed record PortalTenantRequest(
    string Name,
    string Address,
    string TelephoneNo,
    string RegistrationNo,
    string TaxOfficeName,
    string TaxNumber,
    string ContactPersonName,
    string ContactPersonTelephoneNo,
    string ContactPersonEmailAddress,
    string? LogoUrl,
    Guid? CountryId,
    decimal? Latitude,
    decimal? Longitude,
    string? ThemeKey = null,
    string? ThemeMode = null);
