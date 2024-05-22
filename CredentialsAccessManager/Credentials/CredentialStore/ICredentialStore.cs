using UserId = System.Guid;
using Username = string;
using Password = string;
using ApiKeyId = string;
using SessionId = string;
using SinglePermission = (string service, string permission);

using CredentialsAccessManager.Session;
using AuthProvider.CamInterface;

namespace CredentialsAccessManager.Credentials.CredentialStore;

public interface ICredentialStore
{
    #region User
    public UserActionResult<UserId> CreateUser(Username username, Password password);
    public AuthorizationResult AuthenticateCredentials(Username username, Password password);
    public AuthorizationResult AuthorizeCredentials(Username username, Password password, SinglePermission permission);
    public UserActionResult<UserId> GetUserIdFromUsername(Username username);
    public UserActionResult<Username> GetUsernameFromUserId(UserId userId);

    // public UserActionResult<Permissions> GetUserPermissions(UserId userId);
    // public UserActionResult GrantUserPermission(UserId userId, Permission permission);
    // public UserActionResult RevokeUserPermission(UserId userId, Permission permission);
    // public UserActionResult<UserInformation> GetUserInformation(UserId userId);
    // public UserActionResult UpdateUserInformation(UserId userId, UserInformation newUserInformation);
    // public UserActionResult UpdateUsername(UserId userId, Username username);
    // public UserActionResult UpdatePassword(UserId userId, Password password);
    #endregion


    #region Session
    public UserActionResult<SessionId> CreateNewSession(UserId userId);
    public AuthorizationResult AuthenticateSession(SessionId sessionId);
    public AuthorizationResult AuthorizeSession(SessionId sessionId, SinglePermission permission);
    public AuthorizationResult AuthenticateStrictSession(SessionId sessionId, Password password);
    public AuthorizationResult AuthorizeStrictSession(SessionId sessionId, Password password, SinglePermission permission);
    public UserActionResult RevokeSessionBySessionId(SessionId sessionId);
    public UserActionResult RevokeSessionBySessionInternalId(UserId userId, int internalSessionId);
    public UserActionResult RevokeAllSessions(UserId userId);
    public UserActionResult<List<ActiveSession>> GetAllSessions(UserId userId);
    #endregion


    #region ApiKey
    public UserActionResult<ApiKeyId> CreateNewApiKey(UserId userId, Permissions permissions);
    public AuthorizationResult AuthenticateApiKey(ApiKeyId apiKeyId);
    public AuthorizationResult AuthorizeApiKey(ApiKeyId apiKeyId, SinglePermission permission);
    public UserActionResult RevokeApiKeyPermissionByApiKeyInternalId(UserId userId, string internalId, SinglePermission permission);
    public UserActionResult DeleteApiKeyByApiKeyInternalId(UserId userId, string internalId);
    public UserActionResult<List<ApiKey>> GetAllApiKeys(UserId userId);
    #endregion
}