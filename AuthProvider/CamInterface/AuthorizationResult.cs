namespace AuthProvider.CamInterface;

public class AuthorizationResult
{
    public bool IsAuthenticated { get; protected set; }
    public bool IsAuthorized { get; protected set; }
    public (string service, string permission)? Permission { get; protected set; }
    public Guid? UserId { get; protected set; }

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

public enum FailureCause
{
    
}