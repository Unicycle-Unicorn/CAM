using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace AuthProvider.Authentication;

public class NullAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    private const string Name = "NullAuthentication";
    private const string DisplayName = "Null Authentication Handler";

    public static void RegisterWithBuilder(AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = Name;
        options.DefaultForbidScheme = Name;
        options.AddScheme<NullAuthenticationHandler>(Name, DisplayName);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() => AuthenticateResult.NoResult();
}
