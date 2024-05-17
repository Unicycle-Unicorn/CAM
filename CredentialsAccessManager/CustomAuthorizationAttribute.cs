using AuthProvider;
using CredentialsAccessManager.Session;
using CredentialsAccessManager.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CredentialsAccessManager;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CustomAuthorizationAttribute : Attribute, IAuthorizationFilter//, IAsyncAuthorizationFilter
{
    private readonly string? Permission;

    public CustomAuthorizationAttribute()
    {
    }

    public CustomAuthorizationAttribute(string permission)
    {
        Permission = permission;
        RegistrationService.RegisterPermission(permission);
    }

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

                    if (!UserStore.HasPermission(session.UserId, RegistrationService.Service, Permission))
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

    }

    //public Task OnAuthorizationAsync(AuthorizationFilterContext context) => throw new NotImplementedException();
}
