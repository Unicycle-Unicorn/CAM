namespace AuthProvider.CamInterface;

public class AuthorizationResult
{
    public bool IsAuthenticated { get; set; }
    public bool IsAuthorized { get; set; }
    public (string service, string permission)? Permission { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }

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

    public static AuthorizationResult Authenticated(Guid userId, string username)
    {
        return new AuthorizationResult()
        {
            IsAuthenticated = true,
            IsAuthorized = false,
            UserId = userId,
            Username = username,
            Permission = null
        };
    }

    public static AuthorizationResult Authorized(Guid userId, string username, (string service, string permission) permission)
    {
        return new AuthorizationResult()
        {
            IsAuthenticated = true,
            IsAuthorized = true,
            UserId = userId,
            Username = username,
            Permission = permission
        };
    }
}