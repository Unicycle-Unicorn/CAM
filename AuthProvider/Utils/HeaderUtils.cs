using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace AuthProvider.Utils;
public static class HeaderUtils
{
    public const string XAuthUser = "X-Auth-User";
    public const string XAuthPass = "X-Auth-Pass";
    public const string XApiKey = "X-Api-Key";
    public const string XAuthCSRF = "X-Auth-CSRF";
    public const string XExceptionCode = "X-Exception-Code";

    public static bool TryGetHeader(HttpRequest request, string name, [NotNullWhen(true)] out string? result)
    {
        result = null;

        if (request.Headers.TryGetValue(name, out var value))
        {
            if (!string.IsNullOrEmpty(value))
            {
                result = value!;
                return true;
            }
        }

        return false;
    }

    public static void AddHeader(HttpResponse response, string name, string value)
    {
        response.Headers.Append(name, value);
    }
}
