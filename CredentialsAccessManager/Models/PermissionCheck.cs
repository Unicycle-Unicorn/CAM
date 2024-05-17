using AuthProvider;

namespace CredentialsAccessManager.Models;

public class PermissionCheck
{
    public required SessionCredentials Session { get; set; }

    public string? Service = null;

    public string? Permission = null;
}
