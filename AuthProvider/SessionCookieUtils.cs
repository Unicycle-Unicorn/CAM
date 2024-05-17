using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace AuthProvider;
public static class SessionCookieUtils
{
    private const string SessionCookieName = "SessionCredentials";

    private static readonly CookieOptions DefaultCookieOptions = new()
    {
        //Domain = "api.unicycleunicorn.net",
        HttpOnly = true,
        IsEssential = true,
        Secure = true,
    };

    public static void AttachSession(HttpResponse response, SessionCredentials session) => response.Cookies.Append(SessionCookieName, session.ToString(), DefaultCookieOptions);

    public static void RemoveSession(HttpResponse response) => response.Cookies.Delete(SessionCookieName);

    public static bool AttemptQuerySession(HttpRequest request, [NotNullWhen(true)] out SessionCredentials? session)
    {
        if (request.Cookies.TryGetValue(SessionCookieName, out string? sessionCookie))
        {
            if (sessionCookie != null)
            {
                session = SessionCredentials.FromString(sessionCookie);
                return session != null;
            }
        }

        session = null;
        return false;
    }
}
