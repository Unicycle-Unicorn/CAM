using AuthProvider.CamInterface;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace AuthProvider.Authentication;
public class SessionAuth : ICamAuthorizer
{
    public void ApplySwaggerGeneration(OpenApiOperation operation)
    {
        operation.AddOptionalRequestHeader(HeaderUtils.XAuthCSRF);
        operation.AddOptionalRequestCookie(CookieUtils.Session);
    }
    public async Task<AuthorizationResult> AuthenticateAsync(HttpRequest request, ICamInterface cam)
    {
        if (CookieUtils.TryGetCookie(request, CookieUtils.Session, out string sessionId) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthCSRF, out string csrf))
        {
            Console.WriteLine($"Recieved Session Id: {sessionId}");
            Console.WriteLine($"Recieved CSRF: {csrf}");
            if (CSRFUtils.VerifyCSRF(sessionId, csrf))
            {
                Console.WriteLine($"CSRF Passed");
                return await cam.AuthenticateSessionAsync(sessionId);
            }
        }

        return AuthorizationResult.Failed();
    }
    public async Task<AuthorizationResult> AuthorizeAsync(HttpRequest request, ICamInterface cam, string permission)
    {
        if (CookieUtils.TryGetCookie(request, CookieUtils.Session, out string sessionId) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthCSRF, out string csrf))
        {
            if (CSRFUtils.VerifyCSRF(sessionId, csrf))
            {
                return await cam.AuthorizeSessionAsync(sessionId, permission);
            }
        }

        return AuthorizationResult.Failed();
    }
}
