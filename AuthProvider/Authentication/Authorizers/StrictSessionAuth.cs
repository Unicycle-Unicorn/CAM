using AuthProvider.AuthModelBinder;
using AuthProvider.CamInterface;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace AuthProvider.Authentication.Authorizers;

public class StrictSessionAuth : ICamAuthorizer
{
    public void ApplySwaggerGeneration(OpenApiOperation operation)
    {
        operation.AddOptionalRequestHeader(HeaderUtils.XAuthCSRF);
        operation.AddOptionalRequestHeader(HeaderUtils.XAuthPass);
        operation.AddOptionalRequestCookie(CookieUtils.Session);
    }
    public async Task<(AuthorizationResult, Action)> AuthenticateAsync(HttpContext context, ICamInterface cam)
    {
        HttpRequest request = context.Request;

        if (CookieUtils.TryGetCookie(request, CookieUtils.Session, out string sessionId) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthPass, out string password) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthCSRF, out string csrf))
        {
            if (CSRFUtils.VerifyCSRF(sessionId, csrf))
            {
                AuthorizationResult result = await cam.AuthenticateStrictSessionAsync(sessionId, password);
                return (result, AddItemsAction(context, result, sessionId));
            }
        }

        AuthorizationResult failure = AuthorizationResult.Failed();
        return (failure, AddItemsAction(context, failure, sessionId));
    }
    public async Task<(AuthorizationResult, Action)> AuthorizeAsync(HttpContext context, ICamInterface cam, string permission)
    {
        HttpRequest request = context.Request;

        if (CookieUtils.TryGetCookie(request, CookieUtils.Session, out string sessionId) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthPass, out string password) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthCSRF, out string csrf))
        {
            if (CSRFUtils.VerifyCSRF(sessionId, csrf))
            {
                AuthorizationResult result = await cam.AuthorizeStrictSessionAsync(sessionId, password, permission);
                return (result, AddItemsAction(context, result, sessionId));
            }
        }

        AuthorizationResult failure = AuthorizationResult.Failed();
        return (failure, AddItemsAction(context, failure, sessionId));
    }

    private Action AddItemsAction(HttpContext context, AuthorizationResult result, string sessionId)
    {
        return () =>
        {
            ItemUtils.Add<AuthType>(context, GetType().Name);

            if (result.IsAuthenticated)
            {
                ItemUtils.Add<AuthUserId>(context, result.UserId!);
                ItemUtils.Add<AuthSessionId>(context, sessionId);
                ItemUtils.Add<AuthUsername>(context, result.Username!);
            }

            if (result.IsAuthorized)
            {
                ItemUtils.Add<AuthPermission>(context, result.Permission!.Value.permission);
                ItemUtils.Add<AuthPermissionService>(context, result.Permission!.Value.service);
            }
        };
    }

    public static Type[] ProvidedItemsDuringAuthorization() => [typeof(AuthType), typeof(AuthUserId), typeof(AuthSessionId), typeof(AuthUsername), typeof(AuthPermission), typeof(AuthPermissionService)];
    public static Type[] ProvidedItemsDuringAuthentication() => [typeof(AuthType), typeof(AuthUserId), typeof(AuthSessionId), typeof(AuthUsername)];
}
