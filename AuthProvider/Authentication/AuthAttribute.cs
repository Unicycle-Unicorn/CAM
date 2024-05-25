using AuthProvider.Authentication.Authorizers;
using AuthProvider.CamInterface;
using AuthProvider.RuntimePrecheck;
using AuthProvider.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace AuthProvider;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public abstract class AuthAttribute : PrecheckMethodAttribute, IAsyncAuthorizationFilter
{
    protected readonly string? Permission;
    protected readonly bool WithPermission;

    public AuthAttribute()
    {
        WithPermission = false;
        Permission = null;
    }

    public AuthAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission, nameof(permission));
        ICamInterface.RegisterPermission(permission);
        Permission = permission;
        WithPermission = true;
    }

    public void GenerateSwagger(OpenApiOperation operation, OperationFilterContext context)
    {
        GenerateAuthorizerSwagger(operation, context);

        if (WithPermission)
        {
            operation.AddResponse("403", "Forbidden");
        }

        operation.AddResponse("401", "Unauthorized");
    }

    protected abstract void GenerateAuthorizerSwagger(OpenApiOperation operation, OperationFilterContext context);

    public abstract HashSet<Type> GetFromAuthAvailableTypes();

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        (AuthorizationResult authResult, Action addItems) result;

        if (WithPermission)
        {
            result = await AuthorizeAsync(context.HttpContext); 
        }
        else
        {
            result = await AuthenticateAsync(context.HttpContext);
        }

        // Add the authorizer's items to the context's items
        result.addItems();

        // If not authenticated, we always want to fail
        if (!result.authResult.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Only fail if this endpoint requires permission and the user is not authorized for that permission
        if (WithPermission && !result.authResult.IsAuthorized)
        {
            context.Result = new ForbidResult();
            return;
        }
    }

    public abstract Task<(AuthorizationResult authResult, Action addItems)> AuthorizeAsync(HttpContext context);

    public abstract Task<(AuthorizationResult authResult, Action addItems)> AuthenticateAsync(HttpContext context);
}

public sealed class AuthAttribute<PrimaryAuthorizer> : AuthAttribute where PrimaryAuthorizer : ICamAuthorizer, new()
{
    private readonly ICamAuthorizer Auth = new PrimaryAuthorizer();

    public AuthAttribute() : base() { }

    public AuthAttribute(string permission) : base(permission) { }

    public override HashSet<Type> GetFromAuthAvailableTypes()
    {
        if (WithPermission)
        {
            return [.. PrimaryAuthorizer.ProvidedItemsDuringAuthorization()];
        } else
        {
            return [.. PrimaryAuthorizer.ProvidedItemsDuringAuthentication()];
        }
    }

    protected override void GenerateAuthorizerSwagger(OpenApiOperation operation, OperationFilterContext context)
    {
        Auth.ApplySwaggerGeneration(operation);
    }

    public override async Task<(AuthorizationResult authResult, Action addItems)> AuthenticateAsync(HttpContext context)
    {
        ICamInterface camInterface = context.RequestServices.GetService<ICamInterface>()!;
        return await Auth.AuthenticateAsync(context, camInterface);
    }

    public override async Task<(AuthorizationResult authResult, Action addItems)> AuthorizeAsync(HttpContext context)
    {
        ICamInterface camInterface = context.RequestServices.GetService<ICamInterface>()!;
        return await Auth.AuthorizeAsync(context, camInterface, Permission!);
    }

    public override bool PreCheck(ControllerActionDescriptor action, ILogger logger) => false;
}


public sealed class AuthAttribute<PrimaryAuthorizer, SecondaryAuthorizer> : AuthAttribute where PrimaryAuthorizer : ICamAuthorizer, new() where SecondaryAuthorizer : ICamAuthorizer, new()
{
    private readonly ICamAuthorizer PrimaryAuth = new PrimaryAuthorizer();
    private readonly ICamAuthorizer SecondaryAuth = new SecondaryAuthorizer();

    public override HashSet<Type> GetFromAuthAvailableTypes()
    {
        if (WithPermission)
        {
            var p = PrimaryAuthorizer.ProvidedItemsDuringAuthorization();
            var s = SecondaryAuthorizer.ProvidedItemsDuringAuthorization();
            return [.. p, ..s];
        }
        else
        {
            var p = PrimaryAuthorizer.ProvidedItemsDuringAuthentication();
            var s = SecondaryAuthorizer.ProvidedItemsDuringAuthentication();
            return [.. p, .. s];
        }
    }

    public override bool PreCheck(ControllerActionDescriptor action, ILogger logger)
    {
        bool shouldError = false;
        if (typeof(PrimaryAuthorizer) == typeof(SecondaryAuthorizer))
        {
            logger.LogError($"{action!.DisplayName} should not have duplicate AuthAttribute generics but it's signature is AuthAttribute<{typeof(PrimaryAuthorizer).Name}, {typeof(SecondaryAuthorizer).Name}>");
            return true;
        }

        if (typeof(SecondaryAuthorizer) == typeof(SessionAuth))
        {
            logger.LogWarning($"{action!.DisplayName} may provide better results if it's PrimaryAuthorizer is of type {typeof(SecondaryAuthorizer).Name} due to authorization short circuiting. ie: AuthAttribute<{typeof(SecondaryAuthorizer).Name}, {typeof(PrimaryAuthorizer).Name}>");
        }

        // Consider adding more checks like compatibility like StrictSession w/ Session

        return shouldError;
    }

    public AuthAttribute() : base() {}

    public AuthAttribute(string permission) : base(permission) {}

    protected override void GenerateAuthorizerSwagger(OpenApiOperation operation, OperationFilterContext context)
    {
        PrimaryAuth.ApplySwaggerGeneration(operation);
        SecondaryAuth.ApplySwaggerGeneration(operation);
    }

    public override async Task<(AuthorizationResult authResult, Action addItems)> AuthenticateAsync(HttpContext context)
    {
        ICamInterface camInterface = context.RequestServices.GetService<ICamInterface>()!;

        (AuthorizationResult authResult, Action addItems) primary = await PrimaryAuth.AuthenticateAsync(context, camInterface);
        
        if (primary.authResult.IsAuthenticated)
        {
            // Primary authentication passed, we don't need to try the secondary option
            return primary;
        }

        (AuthorizationResult authResult, Action addItems) secondary = await SecondaryAuth.AuthenticateAsync(context, camInterface);
        return secondary;
    }

    public override async Task<(AuthorizationResult authResult, Action addItems)> AuthorizeAsync(HttpContext context)
    {
        ICamInterface camInterface = context.RequestServices.GetService<ICamInterface>()!;

        (AuthorizationResult authResult, Action addItems) primary = await PrimaryAuth.AuthorizeAsync(context, camInterface, Permission!);

        if (primary.authResult.IsAuthorized)
        {
            // Primary authorization passed, we don't need to try the secondary option
            return primary;
        }

        (AuthorizationResult authResult, Action addItems) secondary = await SecondaryAuth.AuthorizeAsync(context, camInterface, Permission!);
        if (secondary.authResult.IsAuthorized)
        {
            // Secondary authorization passed
            return secondary;
        }

        // Need to choose the one that atleast passed authentication
        
        if (primary.authResult.IsAuthenticated)
        {
            // Return primary because it passed authentication
            return primary;
        }

        if (secondary.authResult.IsAuthenticated)
        {
            // Return secondary because it passed authentication
            return secondary;
        }

        // Neither pased authentication, just return primary
        return primary;
    }
}