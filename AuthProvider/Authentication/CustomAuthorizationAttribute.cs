using AuthProvider.Authentication;

namespace AuthProvider;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CustomAuthorizationAttribute : Attribute//, IAuthorizationFilter//, IAsyncAuthorizationFilter
{
    private readonly string? Permission;
    private readonly bool WithPermissions;
    private readonly AuthType AuthType;

    public CustomAuthorizationAttribute(AuthType authType)
    {
        AuthType = authType;
        WithPermissions = false;
    }

    public CustomAuthorizationAttribute(AuthType authType, string permission)
    {
        Permission = permission;
        switch (authType)
        {
            case AuthType.CREDENTIALS:
                break;
            case AuthType.SESSION:
                break;
            case AuthType.STRICT_SESSION:
                break;
            case AuthType.API_KEY:
                break;
            case AuthType.STANDARD:
                break;
            default:
                break;
        }
        AuthType = authType;
        WithPermissions = true;
        CamService.RegisterPermission(permission);
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

    }

    //public Task OnAuthorizationAsync(AuthorizationFilterContext context) => throw new NotImplementedException();*/
}
