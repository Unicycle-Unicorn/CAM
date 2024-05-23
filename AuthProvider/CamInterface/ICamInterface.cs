namespace AuthProvider.CamInterface;
public interface ICamInterface
{
    #region Inital Cam Interactions
    public abstract string ServiceName { get; }
    public static readonly HashSet<string> Permissions = [];
    public static void RegisterPermission(string permission) => _ = Permissions.Add(permission);
    public Task Initialize();
    #endregion

    #region Auth
    public Task<AuthorizationResult> AuthenticateCredentialsAsync(string username, string password);
    public Task<AuthorizationResult> AuthenticateSessionAsync(string sessionId);
    public Task<AuthorizationResult> AuthenticateStrictSessionAsync(string sessionId, string password);
    public Task<AuthorizationResult> AuthenticateApiKeyAsync(string apiKey);
    public Task<AuthorizationResult> AuthorizeCredentialsAsync(string username, string password, string permission);
    public Task<AuthorizationResult> AuthorizeSessionAsync(string sessionId, string permission);
    public Task<AuthorizationResult> AuthorizeStrictSessionAsync(string sessionId, string password, string permission);
    public Task<AuthorizationResult> AuthorizeApiKeyAsync(string apiKey, string permission);
    #endregion

    #region User
    public Task<UserActionResult<Guid>> GetUserIdFromUsernameAsync(string username);
    public Task<UserActionResult<string>> GetUsernameFromUserIdAsync(Guid userId);
    #endregion
}
