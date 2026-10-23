 
namespace SorteFederation.Shared.Helpers;

public class DbConnectionStringGenerator
{
    public static string Create(string tenantCode)
    {
        var safeCode = new string(tenantCode
            .Select(character => character switch
            {
                'Ş' or 'ş' => 'S',
                'Ğ' or 'ğ' => 'G',
                'I' or 'İ' => 'I',
                'Ü' or 'ü' => 'U',
                'Ç' or 'ç' => 'C',
                'Ö' or 'ö' => 'O',
                'ı' or 'İ' => 'I',
                _ => character
            })
            .Where(char.IsLetterOrDigit)
            .Take(48)
            .ToArray());

        if (string.IsNullOrWhiteSpace(safeCode))
        {
            safeCode = "Tenant";
        }

        var suffix = DateTime.UtcNow.ToString("yyyyMMdd");
        var databaseName = $"{safeCode.ToUpperInvariant()}_{suffix}";
        return databaseName[..Math.Min(63, databaseName.Length)];
    }
}
