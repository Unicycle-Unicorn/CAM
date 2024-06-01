using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

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

    public static bool TryGetCookie(HttpRequest request, string name, [NotNullWhen(true)] out string? result)
    {
        result = null;

        if (request.Cookies.TryGetValue(name, out string? value))
        {
            if (!string.IsNullOrEmpty(value))
            {
                result = value!;
                return true;
            }
        }

        return false;
    }

    public static void SetCookie(HttpResponse response, string name, string value, CookieOptions options)
    {
        response.Cookies.Append(name, value, options);
    }

    public static void RemoveCookie(HttpResponse response, string name)
    {
        response.Cookies.Delete(name);
    }
}
