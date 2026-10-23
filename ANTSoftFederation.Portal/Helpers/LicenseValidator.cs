
namespace SorteFederation.ANTApplications.Helpers;

public class LicenseValidator
{
    public static async Task<(bool IsValid, EndPointRespondDto Response, string LicenseType, string ModuleName, int? UserDeviceCount)> ValidateLicenseAsync(
        Guid companyId,
        Guid applicationId,
        ANTSoftPortalDbContext db)
    {
        var uuid = AppLiceseService.Get();
        
        var checkForLicence = await db.CompanyApplicationLicences.AsNoTracking().Include(c => c.Company)
                                      .Where(p => p.ApplicationId == applicationId && p.CompanyId == companyId && p.RecordState == RecordState.Active)
                                      .OrderBy(p => p.ApplicationId)
                                      .LastOrDefaultAsync();

        if (checkForLicence == null)
        {
            return (false, new EndPointRespondDto()
            {
                MessageCode = "licence_violations",
                Success = false,
                Token = null
            }, null, null, null);
        }

        if (uuid != null && checkForLicence.AppLicense != uuid)
        {
            return (false, new EndPointRespondDto()
            {
                MessageCode = "licence_violations",
                Success = false,
                Token = null
            }, null, null, null);
        }

        if (checkForLicence.LicenseType == LicenseTypeEnum.expired)
        {
            return (false, new EndPointRespondDto()
            {
                MessageCode = "licence_expired",
                Success = false,
                Token = null,
                Type = nameof(LicenseTypeEnum.expired)
            }, null, null, null);
        }

        var expire = string.Empty; 
        var licenceType = string.Empty;
        var moduleName = string.Empty;
        var userDeviceCount = string.Empty;
        var expiresInSeconds = 0;

        var _expirationDate = checkForLicence.AppLicense;
        string[] parts = _expirationDate.Split(new char[] { '@' }, 5);

        if (parts.Length < 5)
        {
            return (false, new EndPointRespondDto()
            {
                MessageCode = "licence_expired",
                Success = false,
                Token = null
            }, null, null, null);
        }

        var company = checkForLicence.Company.Name;
        licenceType = parts[1];
        expire = parts[2];
        moduleName = parts[3];
        userDeviceCount = parts[4];

        if (string.IsNullOrWhiteSpace(expire))
        {
            return (false, new EndPointRespondDto()
            {
                MessageCode = "licence_violations",
                Success = false,
                Token = null
            }, null, null, null);
        }

        DateTime? dateTime = null;
        int? parsedUserDeviceCount = null;
        try
        {
            dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(expire)).DateTime;
            int.TryParse(userDeviceCount, out int deviceCount);
            parsedUserDeviceCount = deviceCount;
        }
        catch
        {
            return (false, new EndPointRespondDto()
            {
                MessageCode = "licence_expired",
                Success = false,
                Token = null
            }, null, null, null);
        }

        var licType = checkForLicence.LicenseKey.ToString();

        if (checkForLicence.LicenseType == LicenseTypeEnum.yearly || checkForLicence.LicenseType == LicenseTypeEnum.demo)
        {
            if (DateTime.UtcNow.Date >= dateTime.Value.Date)
            {
                return (false, new EndPointRespondDto()
                {
                    MessageCode = "licence_expired",
                    Success = false,
                    Token = null
                }, null, null, null);
            }
        }
        else if (checkForLicence.LicenseType == LicenseTypeEnum.users)
        {
            //var currentUser = await db.Users.CountAsync(f => f.RecordState == RecordState.Active);

            //if (currentUser > parsedUserDeviceCount)
            //{
            //    return (true, new EndPointRespondDto()
            //    {
            //        Success = false,
            //        Token = null,
            //        MessageCode = "licence_violations",
            //        Type = checkForLicence.LicenceType.ToString()
            //    }, licType, company, moduleName, parsedUserDeviceCount);
            //}
        }
        else if (checkForLicence.LicenseType == LicenseTypeEnum.devices)
        {
             
        }

        return (true, new EndPointRespondDto()
        {
            Success = true,
            Token = null,
            MessageCode = "licence_active",
            ExpiresInSeconds = expiresInSeconds,
            Type = checkForLicence.LicenseType.ToString()
        }, licType, moduleName, parsedUserDeviceCount);

    }
}
