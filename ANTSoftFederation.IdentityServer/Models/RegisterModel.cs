
namespace ANTSoftFederation.IdentityServer.Models;

public class RegisterModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Email { get; set; }

    public string DisplayName { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

}
