﻿using AuthProvider.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
namespace AuthProvider;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CustomAuthorizationAttribute<T> : Attribute where T : ICamAuthorizer, new() //, IAuthorizationFilter//, IAsyncAuthorizationFilter
{
    private readonly string? Permission;
    private readonly bool WithPermission;
    private readonly ICamAuthorizer Authorizer;

    public CustomAuthorizationAttribute()
    {
        WithPermission = false;
        Permission = null;
        Authorizer = new T();
    }

    public CustomAuthorizationAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrEmpty(permission, nameof(permission));
        Permission = permission;
        Authorizer = new T();
        WithPermission = true;
        CamService.RegisterPermission(permission!);
    }
    /*
    public void OnAuthorization(AuthorizationFilterContext context)
    {

        if (SessionCookieUtils.AttemptQuerySession(context.HttpContext.Request, out SessionCredentials? session))
        {
            ISessionStore SessionStore = context.HttpContext.RequestServices.GetService<ISessionStore>()!;
            if (SessionStore.AuthenticateSession(session.UserId, session.SessionId))
            {
                context.HttpContext.Features.Set(session);
                // Session is valid - check authorization now too
                if (Permission != null)
                {
                    IUserStore UserStore = context.HttpContext.RequestServices.GetService<IUserStore>()!;

                    if (!UserStore.HasPermission(session.UserId, CamService.Service, Permission))
                    {
                        // User does not have appropriate permission
                        context.Result = new ForbidResult();
                    }
                }

                // User is authenticated and authorized (if endpoint requested permission check)
                return;
            }
        }

        // No session provided
        // Session is incorrect format
        // Session failed to authenticate
        context.Result = new UnauthorizedResult();
        return;

    }*/

    /*
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {

    }*/
}
