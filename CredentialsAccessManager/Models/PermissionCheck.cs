using AuthProvider;
using System.Net;

namespace CredentialsAccessManager.Models;

public class PermissionCheck
{
    public SessionCredentials Session {  get; set; }

    public string? Service = null;

    public string? Permission = null;
}
