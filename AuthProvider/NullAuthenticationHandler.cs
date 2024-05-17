using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace AuthProvider;

public class NullAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string Name = "NullAuthentication";
    private const string DisplayName = "Null Authentication Handler";

    public NullAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    public static void RegisterWithBuilder(AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = Name;
        options.DefaultForbidScheme = Name;
        options.AddScheme<NullAuthenticationHandler>(Name, DisplayName);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() => AuthenticateResult.NoResult();
}
