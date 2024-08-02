using AuthProvider.AuthModelBinder;
using AuthProvider.CamInterface;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace AuthProvider.Authentication.Authorizers;

public class CredentialAuth : ICamAuthorizer
{
    public async Task<(AuthorizationResult, Action)> AuthenticateAsync(HttpContext context, ICamInterface cam)
    {
        HttpRequest request = context.Request;

        if (HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthUser, out string XAuthUser) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthPass, out string XAuthPass))
        {
            AuthorizationResult result = await cam.AuthenticateCredentialsAsync(XAuthUser, XAuthPass);
            return (result, AddItemsAction(context, result));
        }

        AuthorizationResult failure = AuthorizationResult.Failed();
        return (failure, AddItemsAction(context, failure));
    }
    public async Task<(AuthorizationResult, Action)> AuthorizeAsync(HttpContext context, ICamInterface cam, string permission)
    {
        HttpRequest request = context.Request;

        if (HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthUser, out string XAuthUser) && HeaderUtils.TryGetHeader(request, HeaderUtils.XAuthPass, out string XAuthPass))
        {
            AuthorizationResult result = await cam.AuthorizeCredentialsAsync(XAuthUser, XAuthPass, permission);
            return (result, AddItemsAction(context, result));
        }

        AuthorizationResult failure = AuthorizationResult.Failed();
        return (failure, AddItemsAction(context, failure));
    }

    private Action AddItemsAction(HttpContext context, AuthorizationResult result)
    {
        return () =>
        {
            ItemUtils.Add<AuthType>(context, GetType().Name);

            if (result.IsAuthenticated)
            {
                ItemUtils.Add<AuthUserId>(context, result.UserId!);
                ItemUtils.Add<AuthUsername>(context, result.Username!);
            }

            if (result.IsAuthorized)
            {
                ItemUtils.Add<AuthPermission>(context, result.Permission!.Value.permission);
                ItemUtils.Add<AuthPermissionService>(context, result.Permission!.Value.service);
            }
        };
    }

    public void ApplySwaggerGeneration(OpenApiOperation operation)
    {
        operation.AddOptionalRequestHeader(HeaderUtils.XAuthUser);
        operation.AddOptionalRequestHeader(HeaderUtils.XAuthPass);
    }

    public static Type[] ProvidedItemsDuringAuthorization() => [typeof(AuthType), typeof(AuthUserId), typeof(AuthUsername), typeof(AuthPermission), typeof(AuthPermissionService)];
    public static Type[] ProvidedItemsDuringAuthentication() => [typeof(AuthType), typeof(AuthUserId), typeof(AuthUsername)];
}
