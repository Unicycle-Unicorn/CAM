namespace AuthProvider.CamInterface;
public interface ICamInterfaceAsync
{
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

    #region Perm
    public Task RegisterPermissionsAsync(string service, HashSet<string> permissions);
    #endregion
}
