using AuthProvider.AuthModelBinder;
using AuthProvider.CamInterface;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace AuthProvider.Authentication.Authorizers;

public class ApiKeyAuth : ICamAuthorizer
{
    public void ApplySwaggerGeneration(OpenApiOperation operation)
    {
        operation.AddOptionalRequestHeader(HeaderUtils.XApiKey);
    }
    public async Task<(AuthorizationResult, Action)> AuthenticateAsync(HttpContext context, ICamInterface cam)
    {
        HttpRequest request = context.Request;

        if (HeaderUtils.TryGetHeader(request, HeaderUtils.XApiKey, out string apiKey))
        {
            AuthorizationResult result = await cam.AuthenticateApiKeyAsync(apiKey);
            return (result, AddItemsAction(context, result, apiKey));
        }

        AuthorizationResult failure = AuthorizationResult.Failed();
        return (failure, AddItemsAction(context, failure, apiKey));
    }
    public async Task<(AuthorizationResult, Action)> AuthorizeAsync(HttpContext context, ICamInterface cam, string permission)
    {
        HttpRequest request = context.Request;

        if (HeaderUtils.TryGetHeader(request, HeaderUtils.XApiKey, out string apiKey))
        {
            AuthorizationResult result = await cam.AuthorizeApiKeyAsync(apiKey, permission);
            return (result, AddItemsAction(context, result, apiKey));
        }

        AuthorizationResult failure = AuthorizationResult.Failed();
        return (failure, AddItemsAction(context, failure, apiKey));
    }

    private Action AddItemsAction(HttpContext context, AuthorizationResult result, string? apiKey)
    {
        return () =>
        {
            ItemUtils.Add<AuthType>(context, GetType().Name);

            if (result.IsAuthenticated)
            {
                ItemUtils.Add<AuthUserId>(context, result.UserId!);
                ItemUtils.Add<AuthApiKey>(context, apiKey);
                ItemUtils.Add<AuthUsername>(context, result.Username!);
            }

            if (result.IsAuthorized)
            {
                ItemUtils.Add<AuthPermission>(context, result.Permission!.Value.permission);
                ItemUtils.Add<AuthPermissionService>(context, result.Permission!.Value.service);
            }
        };
    }

    public static Type[] ProvidedItemsDuringAuthorization() => [typeof(AuthType), typeof(AuthUserId), typeof(AuthApiKey), typeof(AuthUsername), typeof(AuthPermission), typeof(AuthPermissionService)];
    public static Type[] ProvidedItemsDuringAuthentication() => [typeof(AuthType), typeof(AuthUserId), typeof(AuthApiKey), typeof(AuthUsername)];
}
