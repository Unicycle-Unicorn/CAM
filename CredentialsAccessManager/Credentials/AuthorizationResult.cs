namespace CredentialsAccessManager.Credentials;

public class AuthorizationResult
{
    public bool IsAuthenticated;
    public bool IsAuthorized;
    public (string service, string permission)? Permission;
    public Guid? UserId;

    public static AuthorizationResult Failed()
    {
        return new AuthorizationResult()
        {
            IsAuthenticated = false,
            IsAuthorized = false,
            UserId = null,
            Permission = null
        };
    }

    public static AuthorizationResult Authenticated(Guid userId)
    {
        return new AuthorizationResult()
        {
            IsAuthenticated = true,
            IsAuthorized = false,
            UserId = userId,
            Permission = null
        };
    }

    public static AuthorizationResult Authorized(Guid userId, (string service, string permission) permission)
    {
        return new AuthorizationResult()
        {
            IsAuthenticated = true,
            IsAuthorized = true,
            UserId = userId,
            Permission = permission
        };
    }
}
