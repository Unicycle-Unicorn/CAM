using Microsoft.AspNetCore.Http;

namespace AuthProvider.Utils;
public static class CookieUtils
{
    public const string Session = "Session";
    public const string CSRF = "CSRF";

    public static readonly CookieOptions SecureCookieOptions = new()
    {
        //Domain = "api.unicycleunicorn.net",
        HttpOnly = true,
        IsEssential = true,
        Secure = true,
    };

    public static readonly CookieOptions ScriptableCookieOptions = new()
    {
        //Domain = "api.unicycleunicorn.net",
        HttpOnly = false,
        IsEssential = true,
        Secure = true,
    };

    public static void SetCookie(HttpResponse response, string name, string value, CookieOptions options)
    {
        response.Cookies.Append(name, value, options);
    }

    public static void RemoveCookie(HttpResponse response, string name)
    {
        response.Cookies.Delete(name);
    }
}
