using AuthProvider.Authentication;
using AuthProvider.CamInterface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
namespace AuthProvider;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class AuthAttribute<T> : Attribute, IAsyncAuthorizationFilter, IOperationFilter where T : ICamAuthorizer, new()
{
    private readonly string? Permission;
    private readonly bool WithPermission;
    private readonly ICamAuthorizer Authorizer;

    public AuthAttribute()
    {
        WithPermission = false;
        Permission = null;
        Authorizer = new T();
    }

    public AuthAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission, nameof(permission));
        ICamInterface.RegisterPermission(permission);
        Permission = permission;
        Authorizer = new T();
        WithPermission = true;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var RequestAuthAttribute = context.MethodInfo.GetCustomAttribute<AuthAttribute<T>>();
        if (RequestAuthAttribute != null)
        {
            Authorizer.ApplySwaggerGeneration(operation);

            if (RequestAuthAttribute.WithPermission)
            {
                operation.AddResponse("403", "Forbidden");
            }
            operation.AddResponse("401", "Unauthorized");
        }
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ICamInterface camInterface = context.HttpContext.RequestServices.GetService<ICamInterface>()!;
        AuthorizationResult authResult;

        if (WithPermission)
        {
            authResult = await Authorizer.AuthorizeAsync(context.HttpContext.Request, camInterface, Permission!);
            if (authResult.IsAuthorized)
            {
                context.HttpContext.Features.Set(authResult.UserId!.Value);
            }
            else if (authResult.IsAuthenticated)
            {
                context.HttpContext.Features.Set(authResult.UserId!.Value);
                context.Result = new ForbidResult();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        } else
        {
            authResult = await Authorizer.AuthenticateAsync(context.HttpContext.Request, camInterface);
            if (authResult.IsAuthenticated)
            {
                context.HttpContext.Features.Set(authResult.UserId!.Value);
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
